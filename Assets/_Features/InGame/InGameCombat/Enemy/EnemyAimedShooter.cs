using Sirenix.OdinInspector;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyAimedShooter : MonoBehaviour, IEnemyRuntimeInitializable
{
    [SerializeField, Required] private Transform muzzle;
    [SerializeField, MinValue(1)] private int projectileDamage = 1;
    [SerializeField, MinValue(0.01f)] private float projectileSpeed = 6f;
    [SerializeField, MinValue(0.01f)] private float projectileLifetime = 8f;
    [SerializeField, MinValue(0f)] private float initialDelay = 0.5f;
    [SerializeField, MinValue(0.01f)] private float shotInterval = 1.5f;

    private Transform target;
    private EnemyProjectilePool projectilePool;
    private float nextShotTime;
    private bool isInitialized;

    public void Initialize(in EnemyRuntimeContext context)
    {
        target = context.Player;
        projectilePool = context.ProjectilePool;
        nextShotTime = Time.time + initialDelay;
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized || Time.time < nextShotTime) return;

        Vector2 direction = target.position - muzzle.position;
        if (direction.sqrMagnitude <= Mathf.Epsilon) return;

        projectilePool.Spawn(
            muzzle.position,
            direction.normalized,
            projectileSpeed,
            projectileDamage,
            projectileLifetime,
            gameObject);

        nextShotTime = Time.time + shotInterval;
    }
}
