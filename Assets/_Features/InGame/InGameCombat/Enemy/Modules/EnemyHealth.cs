using System;
using Sirenix.OdinInspector;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField, MinValue(1)] private int maxHealth = 10;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    public event Action<DamageInfo> Damaged;
    public event Action<EnemyHealth> Died;

    private void Awake()
    {
        CurrentHealth = MaxHealth;
        IsDead = false;
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
}
