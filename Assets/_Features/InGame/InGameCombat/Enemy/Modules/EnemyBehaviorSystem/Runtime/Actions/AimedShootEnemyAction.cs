using Sirenix.OdinInspector;
using UnityEngine;

[EnemyBehaviorMenu("공격/플레이어 조준 사격")]
[System.Serializable]
public sealed class AimedShootEnemyAction : EnemyAction
{
    [SerializeField, Required, EnemyBehaviorField("발사 위치")] private Transform muzzle;
    [SerializeField, Required, EnemyBehaviorField("투사체 프리팹")] private EnemyProjectile projectilePrefab;
    [SerializeField, MinValue(1), EnemyBehaviorField("투사체 피해량", Minimum = 1f)] private int projectileDamage = 1;
    [SerializeField, MinValue(0.01f), EnemyBehaviorField("투사체 속도", Minimum = 0.01f)] private float projectileSpeed = 6f;
    [SerializeField, MinValue(0f), EnemyBehaviorField("첫 발사 대기 시간", Minimum = 0f)] private float initialDelay = 0.5f;

    [SerializeField, MinValue(0.01f), HorizontalGroup("Shot Interval"), LabelText("Min"), EnemyBehaviorField("최소 발사 간격", Minimum = 0.01f)]
    private float minimumShotInterval = 3f;

    [SerializeField, MinValue(0.01f), HorizontalGroup("Shot Interval"), LabelText("Max"), EnemyBehaviorField("최대 발사 간격", Minimum = 0.01f)]
    private float maximumShotInterval = 6f;

    [SerializeField, MinValue(1), EnemyBehaviorField("점사 횟수", Minimum = 1f)] private int burstCount = 1;
    [SerializeField, ShowIf(nameof(UsesBurst)), MinValue(0.01f), EnemyBehaviorField("점사 간격", Minimum = 0.01f)] private float burstInterval = 0.1f;
    [SerializeField, MinValue(1), EnemyBehaviorField("동시 발사 수", Minimum = 1f)] private int multishotCount = 1;
    [SerializeField, ShowIf(nameof(UsesMultishot)), MinValue(0f), EnemyBehaviorField("확산 각도", Minimum = 0f)] private float spreadAngle = 30f;

    private float remainingShotCooldown;
    private float remainingBurstCooldown;
    private int remainingBurstCount;

    private bool UsesBurst => burstCount > 1;
    private bool UsesMultishot => multishotCount > 1;

    public override void Enter(in EnemyBehaviorContext context)
    {
        maximumShotInterval = Mathf.Max(minimumShotInterval, maximumShotInterval);
        remainingShotCooldown = initialDelay;
        remainingBurstCooldown = 0f;
        remainingBurstCount = 0;
    }

    public override void Update(in EnemyBehaviorContext context)
    {
        if (remainingBurstCount > 0)
        {
            UpdateBurst(context);
            return;
        }

        remainingShotCooldown -= Time.deltaTime;
        if (remainingShotCooldown > 0f) return;
        if (!TryFireVolley(context)) return;

        remainingBurstCount = burstCount - 1;

        if (remainingBurstCount > 0)
        {
            remainingBurstCooldown = burstInterval;
            return;
        }

        remainingShotCooldown = GetRandomShotInterval();
    }

    private void UpdateBurst(in EnemyBehaviorContext context)
    {
        remainingBurstCooldown -= Time.deltaTime;
        if (remainingBurstCooldown > 0f) return;
        if (!TryFireVolley(context)) return;

        remainingBurstCount--;

        if (remainingBurstCount > 0)
        {
            remainingBurstCooldown = burstInterval;
            return;
        }

        remainingShotCooldown = GetRandomShotInterval();
    }

    private bool TryFireVolley(in EnemyBehaviorContext context)
    {
        Vector2 aimDirection = context.Player.position - muzzle.position;
        if (aimDirection.sqrMagnitude <= Mathf.Epsilon) return false;

        aimDirection.Normalize();
        float startAngle = UsesMultishot ? -spreadAngle * 0.5f : 0f;
        float angleStep = UsesMultishot ? spreadAngle / (multishotCount - 1) : 0f;

        for (int index = 0; index < multishotCount; index++)
        {
            float angleOffset = startAngle + angleStep * index;
            Vector2 direction = Quaternion.Euler(0f, 0f, angleOffset) * aimDirection;
            FireProjectile(context, direction);
        }

        return true;
    }

    private void FireProjectile(in EnemyBehaviorContext context, Vector2 direction)
    {
        EnemyProjectile projectile = Object.Instantiate(
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

    private float GetRandomShotInterval() => Random.Range(minimumShotInterval, maximumShotInterval);
}
