using Sirenix.OdinInspector;
using UnityEngine;

[EnemyBehaviorMenu("자신/체력 비율")]
[System.Serializable]
public sealed class HealthRatioEnemyCondition : EnemyCondition
{
    [SerializeField, EnemyBehaviorField("비교 방식")] private EnemyComparison comparison;
    [SerializeField, Range(0f, 1f), SuffixLabel("비율"), EnemyBehaviorField("기준 체력 비율", Minimum = 0f, Maximum = 1f)] private float ratio = 0.3f;

    public override bool Evaluate(in EnemyBehaviorContext context)
    {
        float currentRatio = (float)context.Health.CurrentHealth / context.Health.MaxHealth;
        return EnemyBehaviorMath.Compare(currentRatio, ratio, comparison);
    }
}
