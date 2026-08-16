using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class BaseEnemyProjectile : EnemyProjectile
{
    [SerializeField, Required] private Rigidbody2D body;
    [SerializeField, Required] private Collider2D bodyCollider;
    [SerializeField] private LayerMask targetLayers;

    protected override void OnLaunched(Vector2 position, Vector2 direction, float speed)
    {
        body.position = position;
        body.rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        body.linearVelocity = direction * speed;
    }

    protected override void StopMovement()
    {
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        body.simulated = false;
    }

    private void FixedUpdate()
    {
        if (!IsActive) return;
        if (RuntimeContext.DespawnBounds.Overlaps(bodyCollider)) return;

        DestroyProjectile();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (TryDamageTarget(other, targetLayers, body.position)) DestroyProjectile();
    }

    private void Reset()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
    }
}