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
    private GameObject source;
    private Vector2 direction;
    private int damage;
    private bool isLaunched;

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
        direction = launchDirection;
        damage = damageAmount;
        source = damageSource;
        isLaunched = true;

        body.position = position;
        body.rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        body.linearVelocity = direction * speed;
    }

    private void FixedUpdate()
    {
        if (!isLaunched) return;

        if (!runtimeContext.DespawnBounds.Overlaps(bodyCollider))
        {
            Despawn();
            return;
        }

        if (!runtimeContext.IsCombatActive)
        {
            StopMovement();
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isLaunched || !runtimeContext.IsCombatActive || !targetLayers.Contains(other.gameObject.layer)) return;

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
        StopMovement();
        release(this);
    }

    private void OnDisable()
    {
        isLaunched = false;
        StopMovement();
        runtimeContext = default;
        source = null;
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
