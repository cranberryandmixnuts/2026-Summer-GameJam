using System;
using Sirenix.OdinInspector;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class EnemyProjectile : MonoBehaviour
{
    [SerializeField, Required] private Rigidbody2D body;
    [SerializeField, Required] private Collider2D bodyCollider;
    [SerializeField] private LayerMask targetLayers;

    private Action<EnemyProjectile> release;
    private EnemyRuntimeContext runtimeContext;
    private CombatBridge combatBridge;
    private GameObject source;
    private Vector2 direction;
    private int damage;
    private bool isLaunched;
    private bool isSubscribedToPlayerDied;

    public void Initialize(Action<EnemyProjectile> releaseAction) => release = releaseAction;

    public void Launch(
        Vector2 position,
        Vector2 launchDirection,
        float speed,
        int damageAmount,
        GameObject damageSource,
        in EnemyRuntimeContext context)
    {
        runtimeContext = context;
        combatBridge = context.CombatBridge;
        direction = launchDirection;
        damage = damageAmount;
        source = damageSource;
        isLaunched = true;

        combatBridge.PlayerDied += HandlePlayerDied;
        isSubscribedToPlayerDied = true;

        body.position = position;
        body.rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        body.linearVelocity = direction * speed;
    }

    private void FixedUpdate()
    {
        if (!isLaunched) return;
        if (runtimeContext.DespawnBounds.Overlaps(bodyCollider)) return;

        Despawn();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isLaunched || !targetLayers.Contains(other.gameObject.layer)) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null) return;

        Vector2 hitPoint = other.ClosestPoint(body.position);
        DamageInfo damageInfo = new(damage, source, hitPoint, direction);

        damageable.TryTakeDamage(damageInfo);
        Despawn();
    }

    private void Despawn()
    {
        if (!isLaunched) return;

        isLaunched = false;
        UnsubscribeFromPlayerDied();
        StopMovement();
        release(this);
    }

    private void OnDisable()
    {
        isLaunched = false;
        UnsubscribeFromPlayerDied();
        StopMovement();
        runtimeContext = default;
        combatBridge = null;
        source = null;
    }

    private void HandlePlayerDied() => Despawn();

    private void UnsubscribeFromPlayerDied()
    {
        if (!isSubscribedToPlayerDied) return;

        combatBridge.PlayerDied -= HandlePlayerDied;
        isSubscribedToPlayerDied = false;
    }

    private void StopMovement()
    {
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
    }

    private void Reset()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
    }
}