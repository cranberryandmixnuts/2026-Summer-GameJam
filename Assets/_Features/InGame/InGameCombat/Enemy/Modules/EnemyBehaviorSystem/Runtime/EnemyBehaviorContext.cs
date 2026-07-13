using UnityEngine;

public readonly struct EnemyBehaviorContext
{
    private readonly EnemyBrain brain;

    public EnemyBrain Brain => brain;
    public GameObject Owner => brain.gameObject;
    public Transform Transform => brain.transform;
    public Rigidbody2D Body => brain.Body;
    public Transform Player => brain.RuntimeContext.Player;
    public Collider2D PlayerCollider => brain.RuntimeContext.PlayerCollider;
    public IEnemyHealthSource Health => brain.Health;
    public float StateElapsedTime => brain.StateElapsedTime;
    public bool ActionsComplete => brain.ActionsComplete;

    public EnemyBehaviorContext(EnemyBrain brain) => this.brain = brain;
}
