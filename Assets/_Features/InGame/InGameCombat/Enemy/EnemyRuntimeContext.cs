using UnityEngine;

public readonly struct EnemyRuntimeContext
{
    public Transform Player { get; }
    public Collider2D PlayerCollider { get; }
    public PlayerHealth PlayerHealth { get; }
    public CombatBridge CombatBridge { get; }
    public CombatBounds CombatBounds { get; }
    public DespawnBounds DespawnBounds { get; }

    public EnemyRuntimeContext(
        Transform player,
        Collider2D playerCollider,
        PlayerHealth playerHealth,
        CombatBridge combatBridge,
        CombatBounds combatBounds,
        DespawnBounds despawnBounds)
    {
        Player = player;
        PlayerCollider = playerCollider;
        PlayerHealth = playerHealth;
        CombatBridge = combatBridge;
        CombatBounds = combatBounds;
        DespawnBounds = despawnBounds;
    }
}

public interface IEnemyRuntimeInitializable
{
    public void Initialize(in EnemyRuntimeContext context);
}