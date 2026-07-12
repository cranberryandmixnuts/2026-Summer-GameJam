using UnityEngine;

public readonly struct EnemyRuntimeContext
{
    public Transform Player { get; }
    public Collider2D PlayerCollider { get; }
    public PlayerHealth PlayerHealth { get; }
    public EnemyProjectilePool ProjectilePool { get; }
    public DespawnBounds DespawnBounds { get; }
    public bool IsCombatActive => !PlayerHealth.IsDead;

    public EnemyRuntimeContext(
        Transform player,
        Collider2D playerCollider,
        PlayerHealth playerHealth,
        EnemyProjectilePool projectilePool,
        DespawnBounds despawnBounds)
    {
        Player = player;
        PlayerCollider = playerCollider;
        PlayerHealth = playerHealth;
        ProjectilePool = projectilePool;
        DespawnBounds = despawnBounds;
    }
}

public interface IEnemyRuntimeInitializable
{
    public void Initialize(in EnemyRuntimeContext context);
}
