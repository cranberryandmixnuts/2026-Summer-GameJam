using UnityEngine;

public enum EnemyMovementAxis
{
    [InspectorName("전체")]
    Both,

    [InspectorName("가로")]
    Horizontal,

    [InspectorName("세로")]
    Vertical
}

public enum EnemyComparison
{
    [InspectorName("이하")]
    LessOrEqual,

    [InspectorName("이상")]
    GreaterOrEqual
}

public static class EnemyBehaviorMath
{
    public static Vector2 ApplyAxis(Vector2 value, EnemyMovementAxis axis)
    {
        return axis switch
        {
            EnemyMovementAxis.Horizontal => new Vector2(value.x, 0f),
            EnemyMovementAxis.Vertical => new Vector2(0f, value.y),
            _ => value
        };
    }

    public static bool Compare(float value, float threshold, EnemyComparison comparison) =>
        comparison == EnemyComparison.LessOrEqual
            ? value <= threshold
            : value >= threshold;
}
