using UnityEngine;

public class CardEffect
{
    private const float PercentFactor = 0.01f;

    public int FireLevel { get; init; }
    public int WaterLevel { get; init; }
    public int ElectricLevel { get; init; }
    public int HealLevel { get; init; }

    public float SizeChangePercent { get; init; }
    public float SizeMultiplier => CalculateSizeMultiplier(SizeChangePercent);
    public float SpeedMutliplier { get; init; } = 1f;
    public float KnockbackDistance { get; init; }

    public float AdditionalMultiplier { get; init; }

    public bool DestroyOnEnemyHit { get; init; }
    public bool DestroysEnemyProjectiles { get; init; }

    public static CardEffect operator +(CardEffect left, CardEffect right) =>
        new()
        {
            FireLevel = left.FireLevel + right.FireLevel,
            WaterLevel = left.WaterLevel + right.WaterLevel,
            ElectricLevel = left.ElectricLevel + right.ElectricLevel,
            HealLevel = left.HealLevel + right.HealLevel,
            SizeChangePercent = left.SizeChangePercent + right.SizeChangePercent,
            SpeedMutliplier = left.SpeedMutliplier * right.SpeedMutliplier,
            KnockbackDistance = left.KnockbackDistance + right.KnockbackDistance,
            AdditionalMultiplier = left.AdditionalMultiplier + right.AdditionalMultiplier,
            DestroyOnEnemyHit = left.DestroyOnEnemyHit || right.DestroyOnEnemyHit,
            DestroysEnemyProjectiles =
                left.DestroysEnemyProjectiles || right.DestroysEnemyProjectiles
        };

    public static float CalculateSizeMultiplier(float sizeChangePercent) =>
        Mathf.Max(0f, 1f + sizeChangePercent * PercentFactor);

    public virtual void OnAttached(Card card) { }
}
