using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[EnemyBehaviorMenu("공격/전역 무작위 원형공격")]
[Serializable]
public sealed class RandomCircleHitscanEnemyAction : EnemyAction
{
    [SerializeField, MinValue(1), EnemyBehaviorField("피해량", Minimum = 1f)] private int damage = 1;
    [SerializeField, MinValue(0.01f), EnemyBehaviorField("원형 공격 반경", Minimum = 0.01f)] private float radius = 0.5f;
    [SerializeField, MinValue(1), HorizontalGroup("Attack Count"), LabelText("Min"), EnemyBehaviorField("최소 공격 개수", Minimum = 1f)]
    private int minimumAttackCount = 3;
    [SerializeField, MinValue(1), HorizontalGroup("Attack Count"), LabelText("Max"), EnemyBehaviorField("최대 공격 개수", Minimum = 1f)]
    private int maximumAttackCount = 6;
    [SerializeField, MinValue(0f), SuffixLabel("초"), EnemyBehaviorField("선딜레이", Minimum = 0f)]
    private float windupDuration = 0.5f;
    [SerializeField, MinValue(0f), SuffixLabel("초"), EnemyBehaviorField("후딜레이", Minimum = 0f)]
    private float recoveryDuration = 0.2f;

    private readonly List<Vector2> attackCenters = new();

    private float elapsedTime;
    private bool hasAttacked;

    public IReadOnlyList<Vector2> AttackCenters => attackCenters;
    public float Radius => radius;

    public override void Enter(in EnemyBehaviorContext context)
    {
        GenerateAttackCenters(context);
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

    private void GenerateAttackCenters(in EnemyBehaviorContext context)
    {
        Vector2 playerRootPosition = context.Player.position;
        Vector2 colliderCenterOffset = (Vector2)context.PlayerCollider.bounds.center - playerRootPosition;
        Vector2 minimumRootPosition = context.Brain.RuntimeContext.CombatBounds.Clamp(
            playerRootPosition,
            new Vector2(float.MinValue, float.MinValue),
            context.PlayerCollider);
        Vector2 maximumRootPosition = context.Brain.RuntimeContext.CombatBounds.Clamp(
            playerRootPosition,
            new Vector2(float.MaxValue, float.MaxValue),
            context.PlayerCollider);
        Vector2 minimumCenter = minimumRootPosition + colliderCenterOffset;
        Vector2 maximumCenter = maximumRootPosition + colliderCenterOffset;
        int attackCount = UnityEngine.Random.Range(
            minimumAttackCount,
            Mathf.Max(minimumAttackCount, maximumAttackCount) + 1);

        attackCenters.Clear();

        for (int index = 0; index < attackCount; index++)
        {
            attackCenters.Add(new Vector2(
                UnityEngine.Random.Range(minimumCenter.x, maximumCenter.x),
                UnityEngine.Random.Range(minimumCenter.y, maximumCenter.y)));
        }
    }

    private void Attack(in EnemyBehaviorContext context)
    {
        hasAttacked = true;

        foreach (Vector2 center in attackCenters)
        {
            Vector2 hitPoint = context.PlayerCollider.ClosestPoint(center);
            if ((hitPoint - center).sqrMagnitude > radius * radius) continue;

            Vector2 direction = (Vector2)context.PlayerCollider.bounds.center - center;
            if (direction.sqrMagnitude <= Mathf.Epsilon) direction = context.Transform.up;

            DamageInfo damageInfo = new(damage, context.Owner, hitPoint, direction.normalized);
            context.Brain.RuntimeContext.PlayerHealth.TryTakeDamage(damageInfo);
            return;
        }
    }
}
