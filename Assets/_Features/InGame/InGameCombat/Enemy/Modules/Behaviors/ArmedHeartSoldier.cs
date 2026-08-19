using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public sealed class ArmedHeartSoldier : BaseBehaviour, IEnemyRuntimeInitializable, IEnemyDifficultyInitializable
{
    private const float PositionToleranceSquared = 0.000001f;
    private const int ProjectileCount = 4;

    private static readonly int MoveAnimationHash = Animator.StringToHash("Move");
    private static readonly int ReadyAnimationHash = Animator.StringToHash("ready");
    private static readonly int RushAnimationHash = Animator.StringToHash("rush");
    private static readonly int AttackAnimationHash = Animator.StringToHash("attack");

    private enum Phase
    {
        Entering,
        Waiting,
        PreparingDash,
        Dashing,
        Recovering
    }

    [TitleGroup("참조")]
    [SerializeField, Required] private Rigidbody2D body;
    [TitleGroup("참조")]
    [SerializeField, Required] private Collider2D bodyCollider;
    [TitleGroup("참조")]
    [SerializeField, Required] private Animator animator;
    [TitleGroup("참조")]
    [SerializeField, Required] private SpriteRenderer spriteRenderer;
    [TitleGroup("참조")]
    [SerializeField, Required] private Transform muzzle;
    [TitleGroup("참조")]
    [SerializeField, Required] private EnemyProjectile projectilePrefab;

    [TitleGroup("등장 연출")]
    [SerializeField, MinValue(0f), SuffixLabel("초")] private float entryDuration = 1.5f;
    [TitleGroup("등장 연출")]
    [SerializeField, MinValue(0f), SuffixLabel("unit/s")] private float entrySpeed = 2f;

    [TitleGroup("공격 주기")]
    [SerializeField, MinValue(0.01f), HorizontalGroup("공격 주기/간격"), LabelText("최소")]
    private float minimumAttackInterval = 3f;
    [TitleGroup("공격 주기")]
    [SerializeField, MinValue(0.01f), HorizontalGroup("공격 주기/간격"), LabelText("최대")]
    private float maximumAttackInterval = 6f;
    [TitleGroup("공격 주기")]
    [SerializeField, MinValue(0f), SuffixLabel("초")] private float dashPreparationDuration = 0.5f;
    [TitleGroup("공격 주기")]
    [SerializeField, MinValue(0f), SuffixLabel("초")] private float recoveryDuration = 0.2f;

    [TitleGroup("관통 돌진")]
    [SerializeField, MinValue(1)] private int dashDamage = 1;
    [TitleGroup("관통 돌진")]
    [SerializeField, MinValue(0.01f)] private float dashSpeed = 8f;
    [TitleGroup("관통 돌진")]
    [SerializeField, MinValue(0.01f)] private float maximumDashDistance = 4f;

    [TitleGroup("4방향 유도 사격")]
    [SerializeField, MinValue(1)] private int projectileDamage = 1;
    [TitleGroup("4방향 유도 사격")]
    [SerializeField, MinValue(0.01f)] private float projectileSpeed = 6f;

    [TitleGroup("애니메이션")]
    [SerializeField, MinValue(0f), SuffixLabel("초")] private float animationTransitionDuration = 0.08f;

    private readonly RaycastHit2D[] castResults = new RaycastHit2D[8];

    private ContactFilter2D playerContactFilter;
    private Phase phase;
    private Vector2 dashDirection;
    private float remainingPhaseTime;
    private float travelledDashDistance;
    private float transitionDelay;
    private float difficultyFactor = 1f;
    private float movementSpeedMultiplier = 1f;
    private int currentAnimationHash;
    private bool hasCompletedEntry;
    private bool hasHitPlayerDuringDash;
    private bool defaultSpriteFlipX;
    private bool playerCollisionWasIgnored;
    private bool collisionOverrideActive;
    private bool isInitialized;
    private bool isRunning;

    public EnemyRuntimeContext RuntimeContext { get; private set; }

    public float DifficultyFactor => difficultyFactor;

    public void InitializeDifficulty(float value) =>
        difficultyFactor = EnemyDifficultyUtility.ClampFactor(value);

    public float TransitionDelay
    {
        get => transitionDelay;
        set => transitionDelay = Mathf.Max(0f, value);
    }

    public float MovementSpeedMultiplier
    {
        get => movementSpeedMultiplier;
        set => movementSpeedMultiplier = Mathf.Max(0f, value);
    }

    private void Awake() => defaultSpriteFlipX = spriteRenderer.flipX;

    public void Initialize(in EnemyRuntimeContext context)
    {
        if (isInitialized)
        {
            StopBehavior();
            RuntimeContext.CombatBridge.PlayerDied -= HandlePlayerDied;
        }

        RuntimeContext = context;
        isInitialized = true;
        hasCompletedEntry = false;
        RuntimeContext.CombatBridge.PlayerDied += HandlePlayerDied;
        StartBehavior();
    }

    private void Update()
    {
        if (!isRunning || phase is Phase.Entering or Phase.Dashing) return;

        remainingPhaseTime -= Time.deltaTime;
        if (remainingPhaseTime > 0f) return;

        switch (phase)
        {
            case Phase.Waiting:
                BeginDashPreparation();
                break;
            case Phase.PreparingDash:
                BeginDash();
                break;
            case Phase.Recovering:
                BeginWaiting(true);
                break;
        }
    }

    private void FixedUpdate()
    {
        if (!isRunning) return;

        switch (phase)
        {
            case Phase.Entering:
                UpdateEntry();
                break;
            case Phase.Dashing:
                UpdateDash();
                break;
        }
    }

    private void BeginEntry()
    {
        phase = Phase.Entering;
        remainingPhaseTime = entryDuration;
        body.linearVelocity = Vector2.zero;
        PlayAnimation(MoveAnimationHash);

        if (remainingPhaseTime <= 0f) CompleteEntry();
    }

    private void UpdateEntry()
    {
        float movementDuration = Mathf.Min(remainingPhaseTime, Time.fixedDeltaTime);
        float movementDistance = entrySpeed * MovementSpeedMultiplier * movementDuration;

        if (movementDistance > Mathf.Epsilon)
            body.MovePosition(body.position + Vector2.down * movementDistance);

        remainingPhaseTime -= Time.fixedDeltaTime;
        if (remainingPhaseTime <= 0f) CompleteEntry();
    }

    private void CompleteEntry()
    {
        hasCompletedEntry = true;
        body.linearVelocity = Vector2.zero;
        BeginDashPreparation();
    }

    private void BeginWaiting(bool includeTransitionDelay)
    {
        phase = Phase.Waiting;
        remainingPhaseTime = Random.Range(minimumAttackInterval, maximumAttackInterval);
        if (includeTransitionDelay) remainingPhaseTime += TransitionDelay;
        PlayAnimation(MoveAnimationHash);
    }

    private void BeginDashPreparation()
    {
        Vector2 playerOffset = (Vector2)RuntimeContext.PlayerCollider.bounds.center - body.position;

        dashDirection = playerOffset.sqrMagnitude > Mathf.Epsilon
            ? playerOffset.normalized
            : Vector2.down;
        phase = Phase.PreparingDash;
        remainingPhaseTime = dashPreparationDuration;
        body.linearVelocity = Vector2.zero;
        PlayAnimation(ReadyAnimationHash);

        if (remainingPhaseTime <= 0f) BeginDash();
    }

    private void BeginDash()
    {
        playerContactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = 1 << RuntimeContext.PlayerCollider.gameObject.layer,
            useTriggers = true
        };

        phase = Phase.Dashing;
        travelledDashDistance = 0f;
        hasHitPlayerDuringDash = false;
        body.linearVelocity = Vector2.zero;

        ApplyDashFacing();
        PlayAnimation(RushAnimationHash);

        if (!IsInsideCombatBounds(body.position))
        {
            CompleteDash();
            return;
        }

        playerCollisionWasIgnored = Physics2D.GetIgnoreCollision(
            bodyCollider,
            RuntimeContext.PlayerCollider);

        collisionOverrideActive = true;
        Physics2D.IgnoreCollision(bodyCollider, RuntimeContext.PlayerCollider, true);
        TryDamagePlayer(0f, out _);
    }

    private void UpdateDash()
    {
        float remainingDistance = maximumDashDistance - travelledDashDistance;

        if (remainingDistance <= Mathf.Epsilon)
        {
            CompleteDash();
            return;
        }

        float requestedDistance = Mathf.Min(
            dashSpeed * MovementSpeedMultiplier * Time.fixedDeltaTime,
            remainingDistance);

        Vector2 currentPosition = body.position;
        Vector2 targetPosition = currentPosition + dashDirection * requestedDistance;
        Vector2 clampedPosition = RuntimeContext.CombatBounds.Clamp(
            currentPosition,
            targetPosition,
            bodyCollider);

        float movementDistance = Vector2.Distance(currentPosition, clampedPosition);
        bool reachedCombatBounds =
            (clampedPosition - targetPosition).sqrMagnitude > PositionToleranceSquared;

        TryDamagePlayer(movementDistance, out _);
        if (!isRunning) return;

        if (movementDistance > Mathf.Epsilon) body.MovePosition(clampedPosition);

        travelledDashDistance += movementDistance;

        if (reachedCombatBounds || maximumDashDistance - travelledDashDistance <= Mathf.Epsilon)
            CompleteDash();
    }

    private void CompleteDash()
    {
        RestorePlayerCollision();
        RestoreDefaultFacing();

        body.linearVelocity = Vector2.zero;
        phase = Phase.Recovering;
        remainingPhaseTime = recoveryDuration;

        PlayAnimation(AttackAnimationHash);
        FireFourWayProjectiles();

        if (remainingPhaseTime <= 0f) BeginWaiting(true);
    }

    private void FireFourWayProjectiles()
    {
        float angleStep = 360f / ProjectileCount;

        for (int index = 0; index < ProjectileCount; index++)
        {
            Vector2 direction = Quaternion.Euler(0f, 0f, angleStep * index) * muzzle.right;
            EnemyProjectile projectile = Instantiate(
                projectilePrefab,
                muzzle.position,
                Quaternion.identity);

            projectile.Launch(
                muzzle.position,
                direction,
                projectileSpeed,
                ScaleDamage(projectileDamage),
                gameObject,
                RuntimeContext,
                RuntimeContext.Player);
        }
    }

    private bool TryDamagePlayer(float castDistance, out float hitDistance)
    {
        hitDistance = 0f;
        if (hasHitPlayerDuringDash) return false;

        Physics2D.IgnoreCollision(bodyCollider, RuntimeContext.PlayerCollider, false);
        bool hasHit = TryGetPlayerHitPoint(castDistance, out Vector2 hitPoint, out hitDistance);
        Physics2D.IgnoreCollision(bodyCollider, RuntimeContext.PlayerCollider, true);

        if (!hasHit) return false;

        hasHitPlayerDuringDash = true;

        DamageInfo damageInfo = new(
            ScaleDamage(dashDamage),
            gameObject,
            hitPoint,
            dashDirection);

        RuntimeContext.PlayerHealth.TryTakeDamage(damageInfo);
        return true;
    }

    private bool TryGetPlayerHitPoint(
        float castDistance,
        out Vector2 hitPoint,
        out float hitDistance)
    {
        if (bodyCollider.Distance(RuntimeContext.PlayerCollider).isOverlapped)
        {
            hitPoint = RuntimeContext.PlayerCollider.ClosestPoint(bodyCollider.bounds.center);
            hitDistance = 0f;
            return true;
        }

        if (castDistance <= Mathf.Epsilon)
        {
            hitPoint = default;
            hitDistance = default;
            return false;
        }

        int hitCount = bodyCollider.Cast(
            dashDirection,
            playerContactFilter,
            castResults,
            castDistance);

        for (int index = 0; index < hitCount; index++)
        {
            if (castResults[index].collider != RuntimeContext.PlayerCollider) continue;

            hitPoint = castResults[index].point;
            hitDistance = castResults[index].distance;
            return true;
        }

        hitPoint = default;
        hitDistance = default;
        return false;
    }

    private bool IsInsideCombatBounds(Vector2 position)
    {
        Vector2 clampedPosition = RuntimeContext.CombatBounds.Clamp(
            body.position,
            position,
            bodyCollider);

        return (clampedPosition - position).sqrMagnitude <= PositionToleranceSquared;
    }

    private void ApplyDashFacing() =>
        spriteRenderer.flipX = dashDirection.x < 0f
            ? !defaultSpriteFlipX
            : defaultSpriteFlipX;

    private void RestoreDefaultFacing() => spriteRenderer.flipX = defaultSpriteFlipX;

    private void PlayAnimation(int stateHash)
    {
        if (currentAnimationHash == stateHash) return;

        currentAnimationHash = stateHash;

        if (animationTransitionDuration <= 0f)
        {
            animator.Play(stateHash, 0, 0f);
            return;
        }

        animator.CrossFadeInFixedTime(
            stateHash,
            animationTransitionDuration,
            0,
            0f);
    }

    private void StartBehavior()
    {
        isRunning = true;
        body.linearVelocity = Vector2.zero;

        if (hasCompletedEntry)
        {
            BeginWaiting(false);
            return;
        }

        BeginEntry();
    }

    private void StopBehavior()
    {
        isRunning = false;
        phase = Phase.Waiting;
        currentAnimationHash = 0;

        RestorePlayerCollision();
        RestoreDefaultFacing();

        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
    }

    private void RestorePlayerCollision()
    {
        if (!collisionOverrideActive) return;

        Physics2D.IgnoreCollision(
            bodyCollider,
            RuntimeContext.PlayerCollider,
            playerCollisionWasIgnored);

        collisionOverrideActive = false;
    }

    private void OnEnable()
    {
        if (!isInitialized || isRunning) return;

        StartBehavior();
    }

    private void OnDisable() => StopBehavior();

    private void OnDestroy()
    {
        if (!isInitialized) return;

        RuntimeContext.CombatBridge.PlayerDied -= HandlePlayerDied;
    }

    private void OnValidate() =>
        maximumAttackInterval = Mathf.Max(
            minimumAttackInterval,
            maximumAttackInterval);

    private void Reset()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        muzzle = transform;
    }

    private int ScaleDamage(int baseDamage) =>
        EnemyDifficultyUtility.ScaleStat(baseDamage, difficultyFactor);

    private void HandlePlayerDied() => StopBehavior();
}