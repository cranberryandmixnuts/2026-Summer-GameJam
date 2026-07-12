using Sirenix.OdinInspector;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class EnemyMover : MonoBehaviour
{
    [SerializeField, Required] private Rigidbody2D body;
    [SerializeField] private Vector2 direction = Vector2.down;
    [SerializeField, MinValue(0f)] private float moveSpeed = 2f;

    private Vector2 normalizedDirection;

    private void Awake() => normalizedDirection = direction.normalized;

    private void FixedUpdate()
    {
        Vector2 targetPosition = body.position + normalizedDirection * (moveSpeed * Time.fixedDeltaTime);
        body.MovePosition(targetPosition);
    }

    private void Reset() => body = GetComponent<Rigidbody2D>();
}
