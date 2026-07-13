using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[EnemyBehaviorMenu("공격/지정 각도 사격")]
[Serializable]
public sealed class PatternShootEnemyAction : EnemyAction
{
    [Serializable]
    private struct ProjectileShot
    {
        [SerializeField, SuffixLabel("°"), EnemyBehaviorField("발사 각도")] private float angle;

        public float Angle => angle;
    }

    [SerializeField, Required, EnemyBehaviorField("발사 위치")] private Transform muzzle;
    [SerializeField, Required, EnemyBehaviorField("투사체 프리팹")] private EnemyProjectile projectilePrefab;
    [SerializeField, MinValue(1), EnemyBehaviorField("투사체 피해량", Minimum = 1f)] private int projectileDamage = 1;
    [SerializeField, MinValue(0.01f), EnemyBehaviorField("투사체 속도", Minimum = 0.01f)] private float projectileSpeed = 6f;
    [SerializeField, EnemyBehaviorField("발사 각도 목록")] private List<ProjectileShot> projectileShots = new();
    [SerializeField, MinValue(0f), EnemyBehaviorField("첫 발사 대기 시간", Minimum = 0f)] private float initialDelay = 0.5f;

    [SerializeField, MinValue(0.01f), HorizontalGroup("Shot Interval"), LabelText("Min"), EnemyBehaviorField("최소 발사 간격", Minimum = 0.01f)]
    private float minimumShotInterval = 3f;

    [SerializeField, MinValue(0.01f), HorizontalGroup("Shot Interval"), LabelText("Max"), EnemyBehaviorField("최대 발사 간격", Minimum = 0.01f)]
    private float maximumShotInterval = 6f;

    private float remainingShotCooldown;

    public override void Enter(in EnemyBehaviorContext context)
    {
        maximumShotInterval = Mathf.Max(minimumShotInterval, maximumShotInterval);
        remainingShotCooldown = initialDelay;
    }

    public override void Update(in EnemyBehaviorContext context)
    {
        remainingShotCooldown -= Time.deltaTime;
        if (remainingShotCooldown > 0f) return;

        FireAllProjectiles(context);
        remainingShotCooldown = UnityEngine.Random.Range(minimumShotInterval, maximumShotInterval);
    }

    private void FireAllProjectiles(in EnemyBehaviorContext context)
    {
        foreach (ProjectileShot projectileShot in projectileShots)
        {
            Vector2 direction = Quaternion.Euler(0f, 0f, projectileShot.Angle) * muzzle.right;
            EnemyProjectile projectile = UnityEngine.Object.Instantiate(
                projectilePrefab,
                muzzle.position,
                Quaternion.identity);
            projectile.Launch(
                muzzle.position,
                direction,
                projectileSpeed,
                projectileDamage,
                context.Owner,
                context.Brain.RuntimeContext);
        }
    }
}
