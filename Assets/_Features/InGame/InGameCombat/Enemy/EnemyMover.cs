using Sirenix.OdinInspector;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class EnemyMover : MonoBehaviour, IEnemyRuntimeInitializable
{
    [SerializeField, Required] private Rigidbody2D body;
    [SerializeField, Required] private Collider2D backwalk;
    [SerializeField, Required] private Collider2D keep;
    [SerializeField, MinValue(0f)] private float approachSpeed = 0.5f;
    [SerializeField, MinValue(0f)] private float backwalkSpeed = 1f;

    private EnemyRuntimeContext runtimeContext;
    private float fixedX;
    private bool isInitialized;

    public void Initialize(in EnemyRuntimeContext context)
    {
        runtimeContext = context;
        fixedX = body.position.x;
        isInitialized = true;
    }

    private void Awake()
    {
        backwalk.isTrigger = true;
        keep.isTrigger = true;
    }

    private void FixedUpdate()
    {
        if (!isInitialized || !runtimeContext.IsCombatActive)
        {
            StopMovement();
            return;
        }

        if (IsPlayerInside(backwalk))
        {
            MoveVertically(1f, backwalkSpeed);
            return;
        }

        if (IsPlayerInside(keep))
        {
            HoldPosition();
            return;
        }

        MoveVertically(-1f, approachSpeed);
    }

    private void MoveVertically(float direction, float speed)
    {
        Vector2 targetPosition = new(
            fixedX,
            body.position.y + direction * speed * Time.fixedDeltaTime);
        body.MovePosition(targetPosition);
    }

    private void OnDisable() => StopMovement();

    private bool IsPlayerInside(Collider2D detectionArea) =>
        detectionArea.Distance(runtimeContext.PlayerCollider).isOverlapped;

    private void HoldPosition()
    {
        StopMovement();
        body.MovePosition(new Vector2(fixedX, body.position.y));
    }

    private void StopMovement()
    {
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
    }

    private void Reset() => body = GetComponent<Rigidbody2D>();
}
