using Sirenix.OdinInspector;
using UnityEngine;

[EnemyBehaviorMenu("이동/플레이어 기준 회피")]
[System.Serializable]
public sealed class DodgePlayerEnemyAction : EnemyAction
{
    [SerializeField, MinValue(0f), EnemyBehaviorField("회피 속도", Minimum = 0f)] private float speed = 3f;
    [SerializeField, MinValue(0f), SuffixLabel("초"), EnemyBehaviorField("회피 시간", Minimum = 0f)] private float duration = 0.4f;

    private Vector2 direction;
    private float elapsedTime;

    public override void Enter(in EnemyBehaviorContext context)
    {
        Vector2 playerDirection = ((Vector2)context.Player.position - context.Body.position).normalized;
        float side = Random.value < 0.5f ? -1f : 1f;
        direction = new Vector2(-playerDirection.y, playerDirection.x) * side;
        elapsedTime = 0f;
    }

    public override void Update(in EnemyBehaviorContext context) => elapsedTime += Time.deltaTime;

    public override void FixedUpdate(in EnemyBehaviorContext context)
    {
        if (elapsedTime >= duration) return;

        context.Body.MovePosition(context.Body.position + direction * speed * Time.fixedDeltaTime);
    }

    public override void Exit(in EnemyBehaviorContext context) => context.Body.linearVelocity = Vector2.zero;

    public override bool IsComplete(in EnemyBehaviorContext context) => elapsedTime >= duration;
}
