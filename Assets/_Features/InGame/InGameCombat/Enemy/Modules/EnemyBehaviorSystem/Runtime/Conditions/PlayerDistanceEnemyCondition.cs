using Sirenix.OdinInspector;
using UnityEngine;

public enum EnemyDistanceMeasurement
{
    [InspectorName("직선 거리")]
    Euclidean,

    [InspectorName("가로 거리")]
    Horizontal,

    [InspectorName("세로 거리")]
    Vertical
}

[EnemyBehaviorMenu("플레이어/거리")]
[System.Serializable]
public sealed class PlayerDistanceEnemyCondition : EnemyCondition
{
    [SerializeField, EnemyBehaviorField("비교 방식")] private EnemyComparison comparison;
    [SerializeField, MinValue(0f), EnemyBehaviorField("기준 거리", Minimum = 0f)] private float distance = 3f;
    [SerializeField, EnemyBehaviorField("거리 측정 방식")] private EnemyDistanceMeasurement measurement;

    public override bool Evaluate(in EnemyBehaviorContext context)
    {
        Vector2 offset = context.Player.position - context.Transform.position;
        float measuredDistance = measurement switch
        {
            EnemyDistanceMeasurement.Horizontal => Mathf.Abs(offset.x),
            EnemyDistanceMeasurement.Vertical => Mathf.Abs(offset.y),
            _ => offset.magnitude
        };

        return EnemyBehaviorMath.Compare(measuredDistance, distance, comparison);
    }
}
