using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

[DisallowMultipleComponent]
public sealed class EnemyProjectilePool : MonoBehaviour
{
    [SerializeField, Required] private EnemyProjectile projectilePrefab;
    [SerializeField, Required] private Transform poolRoot;
    [SerializeField, MinValue(1)] private int initialSize = 32;
    [SerializeField, MinValue(1), ValidateInput(nameof(IsValidMaxSize), "최대 크기는 초기 크기 이상이어야 합니다.")]
    private int maxSize = 256;

    private ObjectPool<EnemyProjectile> pool;

    private void Awake()
    {
        pool = new ObjectPool<EnemyProjectile>(
            CreateProjectile,
            OnTakeFromPool,
            OnReturnedToPool,
            OnDestroyProjectile,
            true,
            initialSize,
            maxSize);

        Prewarm();
    }

    private void OnDestroy() => pool.Clear();

    public EnemyProjectile Spawn(
        Vector2 position,
        Vector2 direction,
        float speed,
        int damage,
        GameObject source,
        in EnemyRuntimeContext context)
    {
        EnemyProjectile projectile = pool.Get();
        projectile.Launch(position, direction, speed, damage, source, context);
        return projectile;
    }

    private void Prewarm()
    {
        List<EnemyProjectile> projectiles = new(initialSize);

        for (int i = 0; i < initialSize; i++) projectiles.Add(pool.Get());

        for (int i = 0; i < projectiles.Count; i++) pool.Release(projectiles[i]);
    }

    private EnemyProjectile CreateProjectile()
    {
        EnemyProjectile projectile = Instantiate(projectilePrefab, poolRoot);
        projectile.Initialize(Release);
        return projectile;
    }

    private void OnTakeFromPool(EnemyProjectile projectile) => projectile.gameObject.SetActive(true);

    private void OnReturnedToPool(EnemyProjectile projectile) => projectile.gameObject.SetActive(false);

    private void OnDestroyProjectile(EnemyProjectile projectile) => Destroy(projectile.gameObject);

    private void Release(EnemyProjectile projectile) => pool.Release(projectile);

    private bool IsValidMaxSize(int value) => value >= initialSize;
}
