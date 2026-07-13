using Sirenix.OdinInspector;
using UnityEngine;

[EnemyBehaviorMenu("이동/플레이어에게서 도망")]
[System.Serializable]
public sealed class FleeFromPlayerEnemyAction : EnemyAction
{
    [SerializeField, MinValue(0f), EnemyBehaviorField("도주 속도", Minimum = 0f)] private float speed = 1f;
    [SerializeField, EnemyBehaviorField("이동 축")] private EnemyMovementAxis axis;

    public override void FixedUpdate(in EnemyBehaviorContext context)
    {
        Vector2 offset = context.Transform.position - context.Player.position;
        Vector2 direction = EnemyBehaviorMath.ApplyAxis(offset, axis).normalized;
        context.Body.MovePosition(context.Body.position + direction * speed * Time.fixedDeltaTime);
    }

    public override void Exit(in EnemyBehaviorContext context) => context.Body.linearVelocity = Vector2.zero;
}
