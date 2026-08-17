using UnityEngine;

public static class CardEffectApplicator
{
    public static void Apply(
        EnemyHealth enemyHealth,
        CardEffect effect,
        CardProjectileSettings settings,
        GameObject source,
        Vector2 direction,
        int finalProjectileDamage
    )
    {
        ApplyHeal(enemyHealth, effect.HealLevel, settings.HealPercentPerLevel);
        ApplyKnockback(enemyHealth, effect.KnockbackDistance, direction);

        if (effect.FireLevel <= 0 && effect.WaterLevel <= 0 && effect.ElectricLevel <= 0) return;

        if (!enemyHealth.TryGetComponent(out EnemyCardStatusEffects statusEffects)) statusEffects = enemyHealth.gameObject.AddComponent<EnemyCardStatusEffects>();

        statusEffects.Apply(effect, settings, source, finalProjectileDamage);
    }

    private static void ApplyHeal(EnemyHealth enemyHealth, int level, float percentPerLevel)
    {
        if (level <= 0) return;

        float healRatio = percentPerLevel * 0.01f * level;
        int healAmount = Mathf.RoundToInt(enemyHealth.MaxHealth * healRatio);
        enemyHealth.TryHeal(healAmount);
    }

    private static void ApplyKnockback(
        EnemyHealth enemyHealth,
        float distance,
        Vector2 direction
    )
    {
        if (distance <= 0f || direction.sqrMagnitude <= Mathf.Epsilon) return;

        Rigidbody2D body = enemyHealth.GetComponentInParent<Rigidbody2D>();
        if (body == null) return;

        body.position += direction.normalized * distance;
    }
}
