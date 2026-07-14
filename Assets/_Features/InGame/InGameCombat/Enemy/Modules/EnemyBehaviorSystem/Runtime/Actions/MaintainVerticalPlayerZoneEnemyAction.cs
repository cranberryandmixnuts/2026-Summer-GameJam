using Sirenix.OdinInspector;
using UnityEngine;

[EnemyBehaviorMenu("이동/플레이어 수직 거리 유지")]
[System.Serializable]
public sealed class MaintainVerticalPlayerZoneEnemyAction : EnemyAction
{
    [SerializeField, Required, EnemyBehaviorField("후퇴 감지 영역")] private Collider2D retreatZone;
    [SerializeField, Required, EnemyBehaviorField("정지 감지 영역")] private Collider2D holdZone;
    [SerializeField, MinValue(0f), EnemyBehaviorField("접근 속도", Minimum = 0f)] private float approachSpeed = 0.5f;
    [SerializeField, MinValue(0f), EnemyBehaviorField("후퇴 속도", Minimum = 0f)] private float retreatSpeed = 1f;

    private float fixedX;

    public override void Enter(in EnemyBehaviorContext context) => fixedX = context.Body.position.x;

    public override void FixedUpdate(in EnemyBehaviorContext context)
    {
        if (retreatZone.Distance(context.PlayerCollider).isOverlapped)
        {
            Move(context, 1f, retreatSpeed);
            return;
        }

        if (holdZone.Distance(context.PlayerCollider).isOverlapped)
        {
            context.Body.MovePosition(new Vector2(fixedX, context.Body.position.y));
            return;
        }

        Move(context, -1f, approachSpeed);
    }

    public override void Exit(in EnemyBehaviorContext context) => context.Body.linearVelocity = Vector2.zero;

    private void Move(in EnemyBehaviorContext context, float direction, float speed)
    {
        Rigidbody2D body = context.Body;
        body.MovePosition(new Vector2(
            fixedX,
            body.position.y + direction * speed * context.MovementSpeedMultiplier * Time.fixedDeltaTime));
    }
}
