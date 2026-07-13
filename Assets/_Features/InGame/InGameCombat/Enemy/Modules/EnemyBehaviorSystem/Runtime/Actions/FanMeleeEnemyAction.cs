using System;
using Sirenix.OdinInspector;
using UnityEngine;

[EnemyBehaviorMenu("공격/플레이어 조준 부채꼴 근접공격")]
[Serializable]
public sealed class FanMeleeEnemyAction : EnemyAction
{
    [SerializeField, Required, EnemyBehaviorField("공격 원점")] private Transform attackOrigin;
    [SerializeField, MinValue(1), EnemyBehaviorField("피해량", Minimum = 1f)] private int damage = 1;
    [SerializeField, MinValue(0.01f), EnemyBehaviorField("공격 거리", Minimum = 0.01f)] private float range = 2f;
    [SerializeField, Range(0f, 360f), SuffixLabel("°"), EnemyBehaviorField("부채꼴 각도", Minimum = 0f, Maximum = 360f)]
    private float angle = 90f;
    [SerializeField, MinValue(0f), SuffixLabel("초"), EnemyBehaviorField("선딜레이", Minimum = 0f)]
    private float windupDuration = 0.3f;
    [SerializeField, MinValue(0f), SuffixLabel("초"), EnemyBehaviorField("후딜레이", Minimum = 0f)]
    private float recoveryDuration = 0.2f;

    private readonly RaycastHit2D[] raycastResults = new RaycastHit2D[8];

    private ContactFilter2D playerContactFilter;
    private Vector2 attackDirection;
    private float elapsedTime;
    private bool hasAttacked;

    public override void Enter(in EnemyBehaviorContext context)
    {
        Vector2 origin = attackOrigin.position;
        Vector2 playerOffset = (Vector2)context.PlayerCollider.bounds.center - origin;

        attackDirection = playerOffset.sqrMagnitude > Mathf.Epsilon
            ? playerOffset.normalized
            : (Vector2)attackOrigin.right;
        playerContactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = 1 << context.PlayerCollider.gameObject.layer,
            useTriggers = true
        };
        elapsedTime = 0f;
        hasAttacked = false;

        if (windupDuration <= 0f) Attack(context);
    }

    public override void Update(in EnemyBehaviorContext context)
    {
        elapsedTime += Time.deltaTime;
        if (!hasAttacked && elapsedTime >= windupDuration) Attack(context);
    }

    public override bool IsComplete(in EnemyBehaviorContext context) =>
        hasAttacked && elapsedTime >= windupDuration + recoveryDuration;

    private void Attack(in EnemyBehaviorContext context)
    {
        hasAttacked = true;
        if (!TryGetPlayerHitPoint(context, out Vector2 hitPoint)) return;

        DamageInfo damageInfo = new(damage, context.Owner, hitPoint, attackDirection);
        context.Brain.RuntimeContext.PlayerHealth.TryTakeDamage(damageInfo);
    }

    private bool TryGetPlayerHitPoint(in EnemyBehaviorContext context, out Vector2 hitPoint)
    {
        Vector2 origin = attackOrigin.position;
        Vector2 closestPoint = context.PlayerCollider.ClosestPoint(origin);
        Vector2 closestOffset = closestPoint - origin;
        float halfAngle = angle * 0.5f;

        if (closestOffset.sqrMagnitude <= range * range &&
            (closestOffset.sqrMagnitude <= Mathf.Epsilon || Vector2.Angle(attackDirection, closestOffset) <= halfAngle))
        {
            hitPoint = closestPoint;
            return true;
        }

        if (TryRaycastPlayer(context, origin, -halfAngle, out hitPoint)) return true;
        if (halfAngle > 0f && TryRaycastPlayer(context, origin, halfAngle, out hitPoint)) return true;

        hitPoint = default;
        return false;
    }

    private bool TryRaycastPlayer(
        in EnemyBehaviorContext context,
        Vector2 origin,
        float angleOffset,
        out Vector2 hitPoint)
    {
        Vector2 direction = Quaternion.Euler(0f, 0f, angleOffset) * attackDirection;
        int hitCount = Physics2D.Raycast(
            origin,
            direction,
            playerContactFilter,
            raycastResults,
            range);

        for (int index = 0; index < hitCount; index++)
        {
            if (raycastResults[index].collider != context.PlayerCollider) continue;

            hitPoint = raycastResults[index].point;
            return true;
        }

        hitPoint = default;
        return false;
    }
}
