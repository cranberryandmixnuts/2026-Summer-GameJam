using UnityEngine;

public readonly struct EnemyRuntimeContext
{
    public Transform Player { get; }
    public EnemyProjectilePool ProjectilePool { get; }

    public EnemyRuntimeContext(Transform player, EnemyProjectilePool projectilePool)
    {
        Player = player;
        ProjectilePool = projectilePool;
    }
}

public interface IEnemyRuntimeInitializable
{
    public void Initialize(in EnemyRuntimeContext context);
}
