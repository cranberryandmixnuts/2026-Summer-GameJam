using System;
using Sirenix.OdinInspector;
using UnityEngine;

public sealed class EnemyHealth : MonoBehaviour, IDamageable, IEnemyDifficultyInitializable
{
    [SerializeField, MinValue(1)] private int maxHealth = 10;

    private int baseMaxHealth;
    private bool hasCapturedBaseMaxHealth;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    public event Action<DamageInfo> Damaged;
    public event Action<int> Healed;
    public event Action<EnemyHealth> Died;

    private void Awake()
    {
        CaptureBaseMaxHealth();
        ResetHealth();
    }

    public void InitializeDifficulty(float difficultyFactor)
    {
        CaptureBaseMaxHealth();
        maxHealth = EnemyDifficultyUtility.ScaleStat(baseMaxHealth, difficultyFactor);
        ResetHealth();
    }

    public bool TryTakeDamage(in DamageInfo damageInfo)
    {
        if (damageInfo.Amount <= 0 || IsDead) return false;

        CurrentHealth = Mathf.Max(0, CurrentHealth - damageInfo.Amount);
        Damaged?.Invoke(damageInfo);

        if (CurrentHealth > 0) return true;

        IsDead = true;
        Died?.Invoke(this);
        Destroy(gameObject);
        return true;
    }

    public bool TryHeal(int amount)
    {
        if (amount <= 0 || IsDead || CurrentHealth >= MaxHealth) return false;

        int previousHealth = CurrentHealth;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        Healed?.Invoke(CurrentHealth - previousHealth);
        return true;
    }

    private void CaptureBaseMaxHealth()
    {
        if (hasCapturedBaseMaxHealth) return;

        baseMaxHealth = maxHealth;
        hasCapturedBaseMaxHealth = true;
    }

    private void ResetHealth()
    {
        CurrentHealth = MaxHealth;
        IsDead = false;
    }
}
