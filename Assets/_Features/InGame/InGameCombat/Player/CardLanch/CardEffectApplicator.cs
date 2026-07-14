using UnityEngine;

public static class CardEffectApplicator
{
    public static void Apply(
        EnemyHealth enemyHealth,
        CardEffect effect,
        CardProjectileSettings settings,
        GameObject source
    )
    {
        ApplyHeal(enemyHealth, effect.HealLevel, settings.HealPercentPerLevel);

        if (effect.FireLevel <= 0 && effect.WaterLevel <= 0 && effect.ElectricLevel <= 0) return;

        if (!enemyHealth.TryGetComponent(out EnemyCardStatusEffects statusEffects)) statusEffects = enemyHealth.gameObject.AddComponent<EnemyCardStatusEffects>();

        statusEffects.Apply(effect, settings, source);
    }

    private static void ApplyHeal(EnemyHealth enemyHealth, int level, float percentPerLevel)
    {
        if (level <= 0) return;

        float healRatio = percentPerLevel * 0.01f * level;
        int healAmount = Mathf.RoundToInt(enemyHealth.MaxHealth * healRatio);
        enemyHealth.TryHeal(healAmount);
    }
}
