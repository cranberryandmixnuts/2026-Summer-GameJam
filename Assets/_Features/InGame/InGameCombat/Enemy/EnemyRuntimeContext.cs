using UnityEngine;

public readonly struct EnemyRuntimeContext
{
    public Transform Player { get; }
    public Collider2D PlayerCollider { get; }
    public PlayerHealth PlayerHealth { get; }
    public CombatBridge CombatBridge { get; }
    public DespawnBounds DespawnBounds { get; }

    public EnemyRuntimeContext(
        Transform player,
        Collider2D playerCollider,
        PlayerHealth playerHealth,
        CombatBridge combatBridge,
        DespawnBounds despawnBounds)
    {
        Player = player;
        PlayerCollider = playerCollider;
        PlayerHealth = playerHealth;
        CombatBridge = combatBridge;
        DespawnBounds = despawnBounds;
    }
}

public interface IEnemyRuntimeInitializable
{
    public void Initialize(in EnemyRuntimeContext context);
}