using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerMovement : MonoBehaviour
{
    [SerializeField, Required] private Rigidbody2D body;
    [SerializeField, Required] private Collider2D bodyCollider;
    [SerializeField, Required] private PlayerInputReader inputReader;
    [SerializeField, Required] private CombatBridge combatBridge;
    [FormerlySerializedAs("movementBounds")]
    [SerializeField, Required] private CombatBounds combatBounds;
    [SerializeField, MinValue(0f)] private float moveSpeed = 7f;

    private bool canMove = true;

    private void Awake() => combatBridge.PlayerDied += HandlePlayerDied;

    private void FixedUpdate()
    {
        if (!canMove)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 movement = inputReader.Movement;
        if (movement.sqrMagnitude > 1f) movement.Normalize();

        Vector2 currentPosition = body.position;
        Vector2 targetPosition = currentPosition + movement * (moveSpeed * Time.fixedDeltaTime);
        targetPosition = combatBounds.Clamp(currentPosition, targetPosition, bodyCollider);

        body.MovePosition(targetPosition);
    }

    private void OnDisable() => body.linearVelocity = Vector2.zero;

    private void OnDestroy() => combatBridge.PlayerDied -= HandlePlayerDied;

    private void HandlePlayerDied()
    {
        canMove = false;
        body.linearVelocity = Vector2.zero;
    }

    private void Reset()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        inputReader = GetComponent<PlayerInputReader>();
    }
}