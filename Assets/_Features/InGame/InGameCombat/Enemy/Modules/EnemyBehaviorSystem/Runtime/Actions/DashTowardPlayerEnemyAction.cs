using System;
using Sirenix.OdinInspector;
using UnityEngine;

[EnemyBehaviorMenu("공격/플레이어 방향 돌진")]
[Serializable]
public sealed class DashTowardPlayerEnemyAction : EnemyAction
{
    private const float PositionToleranceSquared = 0.000001f;

    [SerializeField, Required, EnemyBehaviorField("몸체 충돌체")] private Collider2D bodyCollider;
    [SerializeField, MinValue(1), EnemyBehaviorField("충돌 피해량", Minimum = 1f)] private int damage = 1;
    [SerializeField, MinValue(0.01f), EnemyBehaviorField("돌진 속도", Minimum = 0.01f)] private float speed = 8f;
    [SerializeField, MinValue(0.01f), EnemyBehaviorField("최대 돌진 거리", Minimum = 0.01f)] private float maximumDistance = 4f;

    private readonly RaycastHit2D[] castResults = new RaycastHit2D[8];

    private ContactFilter2D playerContactFilter;
    private Vector2 direction;
    private float travelledDistance;
    private bool hasHitPlayer;
    private bool isComplete;
    private bool playerCollisionWasIgnored;
    private bool collisionOverrideActive;

    public override void Enter(in EnemyBehaviorContext context)
    {
        Vector2 playerOffset = (Vector2)context.PlayerCollider.bounds.center - context.Body.position;

        direction = playerOffset.sqrMagnitude > Mathf.Epsilon
            ? playerOffset.normalized
            : (Vector2)context.Transform.up;
        playerContactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = 1 << context.PlayerCollider.gameObject.layer,
            useTriggers = true
        };
        travelledDistance = 0f;
        hasHitPlayer = false;
        isComplete = !IsInsideCombatBounds(context, context.Body.position);
        collisionOverrideActive = false;
        context.Body.linearVelocity = Vector2.zero;

        if (isComplete) return;

        playerCollisionWasIgnored = Physics2D.GetIgnoreCollision(bodyCollider, context.PlayerCollider);
        collisionOverrideActive = true;
        Physics2D.IgnoreCollision(bodyCollider, context.PlayerCollider, true);

        TryDamagePlayer(context, 0f);
        if (context.Brain.RuntimeContext.PlayerHealth.IsDead) isComplete = true;
    }

    public override void FixedUpdate(in EnemyBehaviorContext context)
    {
        if (isComplete) return;

        float remainingDistance = maximumDistance - travelledDistance;

        if (remainingDistance <= Mathf.Epsilon)
        {
            isComplete = true;
            return;
        }

        float requestedDistance = Mathf.Min(speed * Time.fixedDeltaTime, remainingDistance);
        Vector2 currentPosition = context.Body.position;
        Vector2 targetPosition = currentPosition + direction * requestedDistance;
        Vector2 clampedPosition = context.Brain.RuntimeContext.CombatBounds.Clamp(
            currentPosition,
            targetPosition,
            bodyCollider);
        float movementDistance = Vector2.Distance(currentPosition, clampedPosition);
        bool reachedCombatBounds = (clampedPosition - targetPosition).sqrMagnitude > PositionToleranceSquared;

        TryDamagePlayer(context, movementDistance);

        if (context.Brain.RuntimeContext.PlayerHealth.IsDead)
        {
            isComplete = true;
            return;
        }

        if (movementDistance > Mathf.Epsilon) context.Body.MovePosition(clampedPosition);

        travelledDistance += movementDistance;
        if (reachedCombatBounds || maximumDistance - travelledDistance <= Mathf.Epsilon) isComplete = true;
    }

    public override void Exit(in EnemyBehaviorContext context)
    {
        if (collisionOverrideActive)
        {
            Physics2D.IgnoreCollision(
                bodyCollider,
                context.PlayerCollider,
                playerCollisionWasIgnored);
            collisionOverrideActive = false;
        }

        context.Body.linearVelocity = Vector2.zero;
    }

    public override bool IsComplete(in EnemyBehaviorContext context) => isComplete;

    private void TryDamagePlayer(in EnemyBehaviorContext context, float castDistance)
    {
        if (hasHitPlayer) return;

        Physics2D.IgnoreCollision(bodyCollider, context.PlayerCollider, false);
        bool hasHit = TryGetPlayerHitPoint(context, castDistance, out Vector2 hitPoint);
        Physics2D.IgnoreCollision(bodyCollider, context.PlayerCollider, true);

        if (!hasHit) return;

        hasHitPlayer = true;
        DamageInfo damageInfo = new(damage, context.Owner, hitPoint, direction);
        context.Brain.RuntimeContext.PlayerHealth.TryTakeDamage(damageInfo);
    }

    private bool TryGetPlayerHitPoint(
        in EnemyBehaviorContext context,
        float castDistance,
        out Vector2 hitPoint)
    {
        if (bodyCollider.Distance(context.PlayerCollider).isOverlapped)
        {
            hitPoint = context.PlayerCollider.ClosestPoint(bodyCollider.bounds.center);
            return true;
        }

        if (castDistance <= Mathf.Epsilon)
        {
            hitPoint = default;
            return false;
        }

        int hitCount = bodyCollider.Cast(
            direction,
            playerContactFilter,
            castResults,
            castDistance);

        for (int index = 0; index < hitCount; index++)
        {
            if (castResults[index].collider != context.PlayerCollider) continue;

            hitPoint = castResults[index].point;
            return true;
        }

        hitPoint = default;
        return false;
    }

    private bool IsInsideCombatBounds(in EnemyBehaviorContext context, Vector2 position)
    {
        Vector2 clampedPosition = context.Brain.RuntimeContext.CombatBounds.Clamp(
            context.Body.position,
            position,
            bodyCollider);
        return (clampedPosition - position).sqrMagnitude <= PositionToleranceSquared;
    }
}
