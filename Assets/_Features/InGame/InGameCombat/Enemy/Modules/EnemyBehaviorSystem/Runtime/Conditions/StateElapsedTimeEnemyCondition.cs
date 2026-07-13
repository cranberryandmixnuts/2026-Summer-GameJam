using Sirenix.OdinInspector;
using UnityEngine;

[EnemyBehaviorMenu("상태/경과 시간")]
[System.Serializable]
public sealed class StateElapsedTimeEnemyCondition : EnemyCondition
{
    [SerializeField, EnemyBehaviorField("비교 방식")] private EnemyComparison comparison = EnemyComparison.GreaterOrEqual;
    [SerializeField, MinValue(0f), SuffixLabel("초"), EnemyBehaviorField("기준 시간", Minimum = 0f)] private float duration = 1f;

    public override bool Evaluate(in EnemyBehaviorContext context) =>
        EnemyBehaviorMath.Compare(context.StateElapsedTime, duration, comparison);
}
