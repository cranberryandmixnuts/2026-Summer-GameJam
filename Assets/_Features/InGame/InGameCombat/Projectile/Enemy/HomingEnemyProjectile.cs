using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public sealed class HomingEnemyProjectile : EnemyProjectile
{
    [SerializeField, Required] private Rigidbody2D body;
    [SerializeField, Required] private Collider2D bodyCollider;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField, MinValue(0f), SuffixLabel("°/s")] private float trackingSpeed = 60f;

    private float movementSpeed;

    protected override void OnLaunched(Vector2 position, Vector2 direction, float speed)
    {
        Direction = direction.normalized;
        movementSpeed = speed;
        body.position = position;
        ApplyMovement();
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

        if (!RuntimeContext.DespawnBounds.Overlaps(bodyCollider))
        {
            DestroyProjectile();
            return;
        }

        TrackTarget();
        ApplyMovement();
    }

    private void TrackTarget()
    {
        if (Target == null) return;

        Vector2 targetDirection = (Vector2)Target.position - body.position;
        if (targetDirection.sqrMagnitude <= Mathf.Epsilon) return;

        float currentAngle = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg;
        float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        float angle = Mathf.MoveTowardsAngle(
            currentAngle,
            targetAngle,
            trackingSpeed * Time.fixedDeltaTime);
        float angleInRadians = angle * Mathf.Deg2Rad;
        Direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
    }

    private void ApplyMovement()
    {
        body.rotation = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg - 90f;
        body.linearVelocity = Direction * movementSpeed;
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
