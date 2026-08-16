public class CardEffect
{
    public int FireLevel { get; init; }
    public int WaterLevel { get; init; }
    public int ElectricLevel { get; init; }
    public int HealLevel { get; init; }

    public float SizeMultiplier { get; init; } = 1f;
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
            SizeMultiplier = left.SizeMultiplier * right.SizeMultiplier,
            SpeedMutliplier = left.SpeedMutliplier * right.SpeedMutliplier,
            KnockbackDistance = left.KnockbackDistance + right.KnockbackDistance,
            AdditionalMultiplier = left.AdditionalMultiplier + right.AdditionalMultiplier,
            DestroyOnEnemyHit = left.DestroyOnEnemyHit || right.DestroyOnEnemyHit,
            DestroysEnemyProjectiles =
                left.DestroysEnemyProjectiles || right.DestroysEnemyProjectiles
        };

    public virtual void OnAttached(Card card) { }
}
