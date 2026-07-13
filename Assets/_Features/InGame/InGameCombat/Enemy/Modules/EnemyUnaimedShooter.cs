using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyUnaimedShooter : MonoBehaviour, IEnemyRuntimeInitializable
{
    [System.Serializable]
    private struct ProjectileShot
    {
        [SerializeField, SuffixLabel("°")] private float angle;

        public float Angle => angle;
    }

    [SerializeField, Required] private Transform muzzle;
    [SerializeField, Required] private EnemyProjectile projectilePrefab;
    [SerializeField, MinValue(1)] private int projectileDamage = 1;
    [SerializeField, MinValue(0.01f)] private float projectileSpeed = 6f;
    [SerializeField] private List<ProjectileShot> projectileShots = new();
    [SerializeField, MinValue(0f)] private float initialDelay = 0.5f;

    [SerializeField, MinValue(0.01f), HorizontalGroup("Shot Interval"), LabelText("Min")]
    private float minimumShotInterval = 3f;

    [SerializeField, MinValue(0.01f), HorizontalGroup("Shot Interval"), LabelText("Max")]
    private float maximumShotInterval = 6f;

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

        FireAllProjectiles();
        remainingShotCooldown = GetRandomShotInterval();
    }

    private void FireAllProjectiles()
    {
        foreach (ProjectileShot projectileShot in projectileShots)
        {
            Vector2 direction = Quaternion.Euler(0f, 0f, projectileShot.Angle) * muzzle.right;
            EnemyProjectile projectile = Instantiate(projectilePrefab, muzzle.position, Quaternion.identity);
            projectile.Launch(
                muzzle.position,
                direction,
                projectileSpeed,
                projectileDamage,
                gameObject,
                runtimeContext);
        }
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