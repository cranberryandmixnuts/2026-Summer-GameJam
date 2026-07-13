using Sirenix.OdinInspector;
using UnityEngine;

[EnemyBehaviorMenu("기본/대기")]
[System.Serializable]
public sealed class WaitEnemyAction : EnemyAction
{
    [SerializeField, MinValue(0f), SuffixLabel("초"), EnemyBehaviorField("대기 시간", Minimum = 0f)]
    private float duration = 1f;

    private float elapsedTime;

    public override void Enter(in EnemyBehaviorContext context) => elapsedTime = 0f;

    public override void Update(in EnemyBehaviorContext context) => elapsedTime += Time.deltaTime;

    public override bool IsComplete(in EnemyBehaviorContext context) => elapsedTime >= duration;
}
