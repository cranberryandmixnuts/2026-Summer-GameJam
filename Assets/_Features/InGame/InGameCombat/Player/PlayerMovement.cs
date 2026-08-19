using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerMovement : BaseBehaviour
{
    private enum FacingDirection
    {
        Back,
        Front,
        Side
    }

    private static readonly int BackIdleAnimation = Animator.StringToHash("Base Layer.후면 아이들");
    private static readonly int FrontIdleAnimation = Animator.StringToHash("Base Layer.정면 아이들");
    private static readonly int SideIdleAnimation = Animator.StringToHash("Base Layer.측면 아이들");
    private static readonly int BackRunAnimation = Animator.StringToHash("Base Layer.후면 뛰는거");
    private static readonly int FrontRunAnimation = Animator.StringToHash("Base Layer.정면 뛰는거");
    private static readonly int SideRunAnimation = Animator.StringToHash("Base Layer.측면 뛰는거");

    [SerializeField, Required] private Rigidbody2D body;
    [SerializeField, Required] private Collider2D bodyCollider;
    [SerializeField, Required] private PlayerInputReader inputReader;
    [SerializeField, Required] private CombatBridge combatBridge;
    [FormerlySerializedAs("movementBounds")]
    [SerializeField, Required] private CombatBounds combatBounds;
    [SerializeField, Required] private Animator animator;
    [SerializeField, Required] private SpriteRenderer spriteRenderer;
    [SerializeField, MinValue(0f)] private float moveSpeed = 16f;

    private bool canMove = true;
    private FacingDirection facingDirection = FacingDirection.Front;
    private int currentAnimation;

    private void Awake()
    {
        combatBridge.PlayerDied += HandlePlayerDied;
        PlayAnimation(FrontIdleAnimation);
    }

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
        UpdateAnimation(movement);
    }

    private void OnDisable() => body.linearVelocity = Vector2.zero;

    private void OnDestroy() => combatBridge.PlayerDied -= HandlePlayerDied;

    private void UpdateAnimation(Vector2 movement)
    {
        bool isMoving = movement.sqrMagnitude > 0f;

        if (isMoving)
            UpdateFacingDirection(movement);

        int animation = GetAnimationHash(isMoving);
        PlayAnimation(animation);
    }

    private void UpdateFacingDirection(Vector2 movement)
    {
        if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
        {
            facingDirection = FacingDirection.Side;
            spriteRenderer.flipX = movement.x < 0f;
            return;
        }

        spriteRenderer.flipX = false;
        facingDirection = movement.y > 0f
            ? FacingDirection.Back
            : FacingDirection.Front;
    }

    private int GetAnimationHash(bool isMoving)
    {
        return (facingDirection, isMoving) switch
        {
            (FacingDirection.Back, false) => BackIdleAnimation,
            (FacingDirection.Front, false) => FrontIdleAnimation,
            (FacingDirection.Side, false) => SideIdleAnimation,
            (FacingDirection.Back, true) => BackRunAnimation,
            (FacingDirection.Front, true) => FrontRunAnimation,
            (FacingDirection.Side, true) => SideRunAnimation,
            _ => FrontIdleAnimation
        };
    }

    private void PlayAnimation(int animation)
    {
        if (currentAnimation == animation) return;

        currentAnimation = animation;
        animator.Play(animation, 0);
    }

    private void HandlePlayerDied()
    {
        canMove = false;
        body.linearVelocity = Vector2.zero;
        PlayAnimation(GetAnimationHash(false));
    }

    private void Reset()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        inputReader = GetComponent<PlayerInputReader>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
}