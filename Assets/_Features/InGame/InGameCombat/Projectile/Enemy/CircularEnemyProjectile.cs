using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class CircularEnemyProjectile : EnemyProjectile
{
    [SerializeField, Required] private Rigidbody2D body;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField, MinValue(0.01f)] private float radius = 2f;
    [SerializeField, MinValue(0.01f), SuffixLabel("s")] private float lifetime = 5f;
    [SerializeField] private bool clockwise;

    private Vector2 orbitCenter;
    private float orbitAngle;
    private float angularSpeed;
    private float remainingLifetime;

    protected override void OnLaunched(Vector2 position, Vector2 direction, float speed)
    {
        orbitCenter = position;
        orbitAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angularSpeed = speed / radius * Mathf.Rad2Deg;
        remainingLifetime = lifetime;
        ApplyOrbitPosition(false);
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

        remainingLifetime -= Time.fixedDeltaTime;
        if (remainingLifetime <= 0f)
        {
            DestroyProjectile();
            return;
        }

        float rotationDirection = clockwise ? -1f : 1f;
        orbitAngle += angularSpeed * rotationDirection * Time.fixedDeltaTime;
        ApplyOrbitPosition(true);
    }

    private void ApplyOrbitPosition(bool moveBody)
    {
        float angleInRadians = orbitAngle * Mathf.Deg2Rad;
        Vector2 radialDirection = new(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
        Vector2 position = orbitCenter + radialDirection * radius;
        Direction = clockwise
            ? new Vector2(radialDirection.y, -radialDirection.x)
            : new Vector2(-radialDirection.y, radialDirection.x);

        if (moveBody)
            body.MovePosition(position);
        else
            body.position = position;

        body.rotation = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg - 90f;
    }

    private void OnTriggerEnter2D(Collider2D other) =>
        TryDamageTarget(other, targetLayers, body.position);

    private void Reset() => body = GetComponent<Rigidbody2D>();
}
