using System;
using Sirenix.OdinInspector;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField, MinValue(1)] private int maxHealth = 3;
    [SerializeField, MinValue(0f)] private float invulnerabilityDuration = 0.6f;
    [SerializeField, Required] private CombatBridge combatBridge;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }
    public bool IsInvulnerable => !IsDead && Time.time < invulnerableUntil;

    public event Action<int, int> HealthChanged;
    public event Action<DamageInfo> Damaged;
    public event Action Died;

    private float invulnerableUntil;

    private void Awake() => ResetHealth();

    public bool TryTakeDamage(in DamageInfo damageInfo)
    {
        if (damageInfo.Amount <= 0 || IsDead || IsInvulnerable) return false;

        CurrentHealth = Mathf.Max(0, CurrentHealth - damageInfo.Amount);
        invulnerableUntil = Time.time + invulnerabilityDuration;

        Damaged?.Invoke(damageInfo);
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        combatBridge.PublishPlayerDamaged();

        if (CurrentHealth > 0) return true;

        IsDead = true;
        Died?.Invoke();
        combatBridge.PublishPlayerDied();
        return true;
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead || CurrentHealth >= MaxHealth) return;

        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void ResetHealth()
    {
        CurrentHealth = MaxHealth;
        IsDead = false;
        invulnerableUntil = 0f;
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }
}
