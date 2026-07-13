using System;
using Sirenix.OdinInspector;
using UnityEngine;

[EnemyBehaviorMenu("공격/원형 근접공격")]
[Serializable]
public sealed class CircleMeleeEnemyAction : EnemyAction
{
    [SerializeField, Required, EnemyBehaviorField("공격 원점")] private Transform attackOrigin;
    [SerializeField, MinValue(1), EnemyBehaviorField("피해량", Minimum = 1f)] private int damage = 1;
    [SerializeField, MinValue(0.01f), EnemyBehaviorField("공격 반경", Minimum = 0.01f)] private float radius = 2f;
    [SerializeField, MinValue(0f), SuffixLabel("초"), EnemyBehaviorField("선딜레이", Minimum = 0f)]
    private float windupDuration = 0.3f;
    [SerializeField, MinValue(0f), SuffixLabel("초"), EnemyBehaviorField("후딜레이", Minimum = 0f)]
    private float recoveryDuration = 0.2f;

    private float elapsedTime;
    private bool hasAttacked;

    public override void Enter(in EnemyBehaviorContext context)
    {
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

        Vector2 origin = attackOrigin.position;
        Vector2 hitPoint = context.PlayerCollider.ClosestPoint(origin);
        if ((hitPoint - origin).sqrMagnitude > radius * radius) return;

        Vector2 direction = (Vector2)context.PlayerCollider.bounds.center - origin;
        if (direction.sqrMagnitude <= Mathf.Epsilon) direction = attackOrigin.right;

        DamageInfo damageInfo = new(damage, context.Owner, hitPoint, direction.normalized);
        context.Brain.RuntimeContext.PlayerHealth.TryTakeDamage(damageInfo);
    }
}
