[EnemyBehaviorMenu("상태/행동 완료")]
[System.Serializable]
public sealed class ActionsCompleteEnemyCondition : EnemyCondition
{
    public override bool Evaluate(in EnemyBehaviorContext context) => context.ActionsComplete;
}
