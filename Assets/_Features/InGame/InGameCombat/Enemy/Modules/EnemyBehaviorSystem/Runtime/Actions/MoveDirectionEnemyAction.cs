using Sirenix.OdinInspector;
using UnityEngine;

public enum EnemyDirectionSpace
{
    [InspectorName("월드")]
    World,

    [InspectorName("적 로컬")]
    Self
}

[EnemyBehaviorMenu("이동/지정 방향 이동")]
[System.Serializable]
public sealed class MoveDirectionEnemyAction : EnemyAction
{
    [SerializeField, EnemyBehaviorField("방향")] private Vector2 direction = Vector2.down;
    [SerializeField, MinValue(0f), EnemyBehaviorField("이동 속도", Minimum = 0f)] private float speed = 1f;
    [SerializeField, EnemyBehaviorField("방향 기준")] private EnemyDirectionSpace directionSpace;
    [SerializeField, EnemyBehaviorField("지속 시간 사용")] private bool useDuration;
    [SerializeField, ShowIf(nameof(useDuration)), MinValue(0f), SuffixLabel("초"), EnemyBehaviorField("지속 시간", Minimum = 0f)]
    private float duration = 1f;

    private float elapsedTime;

    public override void Enter(in EnemyBehaviorContext context) => elapsedTime = 0f;

    public override void Update(in EnemyBehaviorContext context) => elapsedTime += Time.deltaTime;

    public override void FixedUpdate(in EnemyBehaviorContext context)
    {
        if (useDuration && elapsedTime >= duration) return;

        Vector2 movementDirection = direction.normalized;

        if (directionSpace == EnemyDirectionSpace.Self) movementDirection = context.Transform.TransformDirection(movementDirection);

        context.Body.MovePosition(
            context.Body.position + movementDirection * speed * context.MovementSpeedMultiplier * Time.fixedDeltaTime);
    }

    public override void Exit(in EnemyBehaviorContext context) => context.Body.linearVelocity = Vector2.zero;

    public override bool IsComplete(in EnemyBehaviorContext context) => useDuration && elapsedTime >= duration;
}
