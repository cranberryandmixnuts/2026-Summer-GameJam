using UnityEngine;

[EnemyBehaviorMenu("이동/정지")]
[System.Serializable]
public sealed class StopMovementEnemyAction : EnemyAction
{
    public override void Enter(in EnemyBehaviorContext context) => Stop(context.Body);

    public override void FixedUpdate(in EnemyBehaviorContext context) => Stop(context.Body);

    private static void Stop(Rigidbody2D body)
    {
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
    }
}
