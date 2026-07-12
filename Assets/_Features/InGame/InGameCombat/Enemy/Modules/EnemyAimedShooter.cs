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
    [SerializeField, MinValue(0.01f)] private float shotInterval = 1.5f;

    private EnemyRuntimeContext runtimeContext;
    private float remainingShotCooldown;
    private bool isInitialized;
    private bool canShoot;

    public void Initialize(in EnemyRuntimeContext context)
    {
        runtimeContext = context;
        remainingShotCooldown = initialDelay;
        canShoot = true;
        isInitialized = true;
        runtimeContext.CombatBridge.PlayerDied += HandlePlayerDied;
    }

    private void Update()
    {
        if (!isInitialized || !canShoot) return;

        remainingShotCooldown -= Time.deltaTime;
        if (remainingShotCooldown > 0f) return;

        Vector2 direction = runtimeContext.Player.position - muzzle.position;
        if (direction.sqrMagnitude <= Mathf.Epsilon) return;

        EnemyProjectile projectile = Instantiate(projectilePrefab, muzzle.position, Quaternion.identity);
        projectile.Launch(
            muzzle.position,
            direction.normalized,
            projectileSpeed,
            projectileDamage,
            gameObject,
            runtimeContext);

        remainingShotCooldown = shotInterval;
    }

    private void OnDestroy()
    {
        if (!isInitialized) return;

        runtimeContext.CombatBridge.PlayerDied -= HandlePlayerDied;
    }

    private void HandlePlayerDied() => canShoot = false;
}