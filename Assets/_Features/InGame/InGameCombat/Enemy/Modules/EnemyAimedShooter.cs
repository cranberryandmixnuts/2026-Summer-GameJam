using Sirenix.OdinInspector;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyAimedShooter : MonoBehaviour, IEnemyRuntimeInitializable
{
    [SerializeField, Required] private Transform muzzle;
    [SerializeField, Required] private EnemyProjectile projectilePrefab;
    [SerializeField, MinValue(1)] private int projectileDamage = 1;
    [SerializeField, MinValue(0.01f)] private float projectileSpeed = 6f;
    [SerializeField, MinValue(0f)] private float initialDelay = 0.5f;

    [SerializeField, MinValue(0.01f), HorizontalGroup("Shot Interval"), LabelText("Min")]
    private float minimumShotInterval = 3f;

    [SerializeField, MinValue(0.01f), HorizontalGroup("Shot Interval"), LabelText("Max")]
    private float maximumShotInterval = 6f;

    [SerializeField, MinValue(1)] private int burstCount = 1;

    [SerializeField, ShowIf(nameof(UsesBurst)), MinValue(0.01f)]
    private float burstInterval = 0.1f;

    [SerializeField, MinValue(1)] private int multishotCount = 1;

    [SerializeField, ShowIf(nameof(UsesMultishot)), MinValue(0f)]
    private float spreadAngle = 30f;

    private EnemyRuntimeContext runtimeContext;
    private float remainingShotCooldown;
    private float remainingBurstCooldown;
    private int remainingBurstCount;
    private bool isInitialized;
    private bool canShoot;

    private bool UsesBurst => burstCount > 1;
    private bool UsesMultishot => multishotCount > 1;

    public void Initialize(in EnemyRuntimeContext context)
    {
        runtimeContext = context;
        remainingShotCooldown = initialDelay;
        remainingBurstCooldown = 0f;
        remainingBurstCount = 0;
        canShoot = true;
        isInitialized = true;
        runtimeContext.CombatBridge.PlayerDied += HandlePlayerDied;
    }

    private void Update()
    {
        if (!isInitialized || !canShoot) return;

        if (remainingBurstCount > 0)
        {
            UpdateBurst();
            return;
        }

        remainingShotCooldown -= Time.deltaTime;
        if (remainingShotCooldown > 0f) return;
        if (!TryFireVolley()) return;

        remainingBurstCount = burstCount - 1;

        if (remainingBurstCount > 0)
        {
            remainingBurstCooldown = burstInterval;
            return;
        }

        remainingShotCooldown = GetRandomShotInterval();
    }

    private void UpdateBurst()
    {
        remainingBurstCooldown -= Time.deltaTime;
        if (remainingBurstCooldown > 0f) return;
        if (!TryFireVolley()) return;

        remainingBurstCount--;

        if (remainingBurstCount > 0)
        {
            remainingBurstCooldown = burstInterval;
            return;
        }

        remainingShotCooldown = GetRandomShotInterval();
    }

    private bool TryFireVolley()
    {
        Vector2 aimDirection = runtimeContext.Player.position - muzzle.position;
        if (aimDirection.sqrMagnitude <= Mathf.Epsilon) return false;

        aimDirection.Normalize();

        float startAngle = UsesMultishot ? -spreadAngle * 0.5f : 0f;
        float angleStep = UsesMultishot ? spreadAngle / (multishotCount - 1) : 0f;

        for (int index = 0; index < multishotCount; index++)
        {
            float angleOffset = startAngle + angleStep * index;
            Vector2 direction = Quaternion.Euler(0f, 0f, angleOffset) * aimDirection;
            FireProjectile(direction);
        }

        return true;
    }

    private void FireProjectile(Vector2 direction)
    {
        EnemyProjectile projectile = Instantiate(projectilePrefab, muzzle.position, Quaternion.identity);
        projectile.Launch(
            muzzle.position,
            direction,
            projectileSpeed,
            projectileDamage,
            gameObject,
            runtimeContext);
    }

    private float GetRandomShotInterval() => Random.Range(minimumShotInterval, maximumShotInterval);

    private void OnValidate()
    {
        maximumShotInterval = Mathf.Max(minimumShotInterval, maximumShotInterval);
    }

    private void OnDestroy()
    {
        if (!isInitialized) return;

        runtimeContext.CombatBridge.PlayerDied -= HandlePlayerDied;
    }

    private void HandlePlayerDied() => canShoot = false;
}