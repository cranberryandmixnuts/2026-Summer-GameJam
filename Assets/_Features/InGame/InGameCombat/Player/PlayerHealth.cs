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
    public bool IsInvulnerable => !isDead && Time.time < invulnerableUntil;

    public event Action<int, int> HealthChanged;
    public event Action<DamageInfo> Damaged;

    private float invulnerableUntil;
    private bool isDead;

    private void Awake() => ResetHealth();

    public bool TryTakeDamage(in DamageInfo damageInfo)
    {
        if (damageInfo.Amount <= 0 || isDead || IsInvulnerable) return false;

        CurrentHealth = Mathf.Max(0, CurrentHealth - damageInfo.Amount);
        invulnerableUntil = Time.time + invulnerabilityDuration;

        Damaged?.Invoke(damageInfo);
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        combatBridge.PublishPlayerDamaged();

        if (CurrentHealth > 0) return true;

        isDead = true;
        combatBridge.PublishPlayerDied();
        return true;
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || isDead || CurrentHealth >= MaxHealth) return;

        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void ResetHealth()
    {
        CurrentHealth = MaxHealth;
        isDead = false;
        invulnerableUntil = 0f;
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }
}