using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public sealed class HeartQueen : MonoBehaviour, IEnemyRuntimeInitializable, IEnemyDifficultyInitializable
{
    private const float PositionToleranceSquared = 0.000001f;

    private enum AttackPattern
    {
        AreaAttack,
        SplitAreaAttack,
        SpinningAttack,
        CloseAttack
    }

    private enum Phase
    {
        Waiting,
        Windup,
        Spinning,
        Dashing,
        Recovering
    }

    [TitleGroup("참조")]
    [SerializeField, Required] private Rigidbody2D body;
    [TitleGroup("참조")]
    [SerializeField, Required] private Animator animator;
    [TitleGroup("참조")]
    [SerializeField, Required] private Collider2D bodyCollider;
    [TitleGroup("참조")]
    [SerializeField, Required] private Transform areaAttackOrigin;
    [TitleGroup("참조")]
    [SerializeField, Required] private Transform spinningAttackOrigin;
    [TitleGroup("참조")]
    [SerializeField, Required] private CircularEnemyProjectile spinningProjectilePrefab;

    [TitleGroup("패턴 주기")]
    [SerializeField, MinValue(0.01f), HorizontalGroup("패턴 주기/간격"), LabelText("최소")]
    private float minimumPatternInterval = 3f;
    [TitleGroup("패턴 주기")]
    [SerializeField, MinValue(0.01f), HorizontalGroup("패턴 주기/간격"), LabelText("최대")]
    private float maximumPatternInterval = 6f;

    [TitleGroup("AreaAttack")]
    [SerializeField, MinValue(1)] private int areaAttackDamage = 1;
    [TitleGroup("AreaAttack")]
    [SerializeField, MinValue(0.01f)] private float areaAttackRadius = 2f;
    [TitleGroup("AreaAttack")]
    [SerializeField, MinValue(0f), SuffixLabel("초")] private float areaAttackWindupDuration = 0.3f;
    [TitleGroup("AreaAttack")]
    [SerializeField, MinValue(0f), SuffixLabel("초")] private float areaAttackRecoveryDuration = 0.2f;

    [TitleGroup("SplitAreaAttack")]
    [SerializeField, MinValue(1)] private int splitAreaAttackDamage = 1;
    [TitleGroup("SplitAreaAttack")]
    [SerializeField, MinValue(0.01f)] private float splitAreaAttackRadius = 0.5f;
    [TitleGroup("SplitAreaAttack")]
    [SerializeField, MinValue(1), HorizontalGroup("SplitAreaAttack/공격 개수"), LabelText("최소")]
    private int minimumSplitAreaAttackCount = 3;
    [TitleGroup("SplitAreaAttack")]
    [SerializeField, MinValue(1), HorizontalGroup("SplitAreaAttack/공격 개수"), LabelText("최대")]
    private int maximumSplitAreaAttackCount = 6;
    [TitleGroup("SplitAreaAttack")]
    [SerializeField, MinValue(0f), SuffixLabel("초")] private float splitAreaAttackWindupDuration = 0.5f;
    [TitleGroup("SplitAreaAttack")]
    [SerializeField, MinValue(0f), SuffixLabel("초")] private float splitAreaAttackRecoveryDuration = 0.2f;

    [TitleGroup("SpinningAttack")]
    [SerializeField, MinValue(1)] private int spinningProjectileDamage = 1;
    [TitleGroup("SpinningAttack")]
    [SerializeField, MinValue(0.01f), LabelText("회전 반경 (프리팹 Radius와 동일)")]
    private float spinningOrbitRadius = 2f;
    [TitleGroup("SpinningAttack")]
    [SerializeField, MinValue(0.01f), SuffixLabel("초")] private float spinningRevolutionDuration = 2f;
    [TitleGroup("SpinningAttack")]
    [SerializeField, SuffixLabel("°")] private float spinningStartAngle;
    [TitleGroup("SpinningAttack")]
    [SerializeField, MinValue(0f), SuffixLabel("초")] private float spinningAttackRecoveryDuration = 0.2f;

    [TitleGroup("CloseAttack")]
    [SerializeField, MinValue(1)] private int closeAttackDamage = 1;
    [TitleGroup("CloseAttack")]
    [SerializeField, MinValue(0.01f)] private float closeAttackSpeed = 8f;
    [TitleGroup("CloseAttack")]
    [SerializeField, MinValue(0.01f)] private float maximumCloseAttackDistance = 8f;
    [TitleGroup("CloseAttack")]
    [SerializeField, MinValue(0f), SuffixLabel("초")] private float closeAttackRecoveryDuration = 0.2f;

    [TitleGroup("애니메이션")]
    [SerializeField] private string idleAnimationStateName;
    [TitleGroup("애니메이션")]
    [SerializeField] private string areaAttackAnimationStateName;
    [TitleGroup("애니메이션")]
    [SerializeField] private string splitAreaAttackAnimationStateName;
    [TitleGroup("애니메이션")]
    [SerializeField] private string spinningAttackAnimationStateName;
    [TitleGroup("애니메이션")]
    [SerializeField] private string closeAttackAnimationStateName;

    private readonly List<Vector2> splitAttackCenters = new();
    private readonly RaycastHit2D[] castResults = new RaycastHit2D[8];

    private ContactFilter2D playerContactFilter;
    private CircularEnemyProjectile activeSpinningProjectile;
    private AttackPattern activePattern;
    private Phase phase;
    private Vector2 dashDirection;
    private float remainingPhaseTime;
    private float travelledDashDistance;
    private float transitionDelay;
    private float difficultyFactor = 1f;
    private float movementSpeedMultiplier = 1f;
    private bool hasHitPlayerDuringDash;
    private bool playerCollisionWasIgnored;
    private bool collisionOverrideActive;
    private bool isInitialized;
    private bool isRunning;

    public EnemyRuntimeContext RuntimeContext { get; private set; }
    public float DifficultyFactor => difficultyFactor;

    public void InitializeDifficulty(float value) =>
        difficultyFactor = EnemyDifficultyUtility.ClampFactor(value);

    public IReadOnlyList<Vector2> SplitAttackCenters => splitAttackCenters;
    public float AreaAttackRadius => areaAttackRadius;
    public float SplitAreaAttackRadius => splitAreaAttackRadius;
    public bool IsAreaAttackTelegraphActive =>
        isRunning && phase == Phase.Windup && activePattern == AttackPattern.AreaAttack;
    public bool IsSplitAreaAttackTelegraphActive =>
        isRunning && phase == Phase.Windup && activePattern == AttackPattern.SplitAreaAttack;
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

    public void Initialize(in EnemyRuntimeContext context)
    {
        if (isInitialized)
        {
            StopBehavior();
            RuntimeContext.CombatBridge.PlayerDied -= HandlePlayerDied;
        }

        RuntimeContext = context;
        isInitialized = true;
        RuntimeContext.CombatBridge.PlayerDied += HandlePlayerDied;
        StartBehavior();
    }

    private void Update()
    {
        if (!isRunning || phase == Phase.Dashing) return;

        remainingPhaseTime -= Time.deltaTime;
        if (remainingPhaseTime > 0f) return;

        switch (phase)
        {
            case Phase.Waiting:
                BeginRandomPattern();
                break;
            case Phase.Windup:
                ExecuteWindupAttack();
                break;
            case Phase.Spinning:
                CompleteSpinningAttack();
                break;
            case Phase.Recovering:
                BeginWaiting(true);
                break;
        }
    }

    private void FixedUpdate()
    {
        if (!isRunning || phase != Phase.Dashing) return;

        UpdateCloseDash();
    }

    private void BeginRandomPattern()
    {
        activePattern = (AttackPattern)Random.Range(0, 4);

        switch (activePattern)
        {
            case AttackPattern.AreaAttack:
                BeginAreaAttack();
                break;
            case AttackPattern.SplitAreaAttack:
                BeginSplitAreaAttack();
                break;
            case AttackPattern.SpinningAttack:
                BeginSpinningAttack();
                break;
            case AttackPattern.CloseAttack:
                BeginCloseAttack();
                break;
        }
    }

    private void BeginAreaAttack()
    {
        phase = Phase.Windup;
        remainingPhaseTime = areaAttackWindupDuration;
        PlayAnimation(areaAttackAnimationStateName);

        if (remainingPhaseTime <= 0f) ExecuteWindupAttack();
    }

    private void BeginSplitAreaAttack()
    {
        GenerateSplitAttackCenters();
        phase = Phase.Windup;
        remainingPhaseTime = splitAreaAttackWindupDuration;
        PlayAnimation(splitAreaAttackAnimationStateName);

        if (remainingPhaseTime <= 0f) ExecuteWindupAttack();
    }

    private void BeginSpinningAttack()
    {
        phase = Phase.Spinning;
        remainingPhaseTime = spinningRevolutionDuration;
        PlayAnimation(spinningAttackAnimationStateName);

        Vector2 direction = Quaternion.Euler(0f, 0f, spinningStartAngle) * spinningAttackOrigin.right;
        float projectileSpeed =
            2f * Mathf.PI * spinningOrbitRadius / spinningRevolutionDuration;

        activeSpinningProjectile = Instantiate(
            spinningProjectilePrefab,
            spinningAttackOrigin.position,
            Quaternion.identity);
        activeSpinningProjectile.Launch(
            spinningAttackOrigin.position,
            direction,
            projectileSpeed,
            ScaleDamage(spinningProjectileDamage),
            gameObject,
            RuntimeContext,
            RuntimeContext.Player);
    }

    private void BeginCloseAttack()
    {
        Vector2 playerOffset = (Vector2)RuntimeContext.PlayerCollider.bounds.center - body.position;

        dashDirection = playerOffset.sqrMagnitude > Mathf.Epsilon
            ? playerOffset.normalized
            : (Vector2)transform.up;
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
        PlayAnimation(closeAttackAnimationStateName);

        if (!IsInsideCombatBounds(body.position))
        {
            CompleteCloseAttack();
            return;
        }

        playerCollisionWasIgnored = Physics2D.GetIgnoreCollision(
            bodyCollider,
            RuntimeContext.PlayerCollider);
        collisionOverrideActive = true;
        Physics2D.IgnoreCollision(bodyCollider, RuntimeContext.PlayerCollider, true);

        bool hitPlayer = TryDamagePlayerWithDash(0f, out _);
        if (!isRunning) return;
        if (hitPlayer) CompleteCloseAttack();
    }

    private void ExecuteWindupAttack()
    {
        if (activePattern == AttackPattern.AreaAttack)
        {
            ApplyAreaAttack();
            BeginRecovery(areaAttackRecoveryDuration);
            return;
        }

        ApplySplitAreaAttack();
        BeginRecovery(splitAreaAttackRecoveryDuration);
    }

    private void ApplyAreaAttack()
    {
        Vector2 origin = areaAttackOrigin.position;
        Vector2 hitPoint = RuntimeContext.PlayerCollider.ClosestPoint(origin);
        if ((hitPoint - origin).sqrMagnitude > areaAttackRadius * areaAttackRadius) return;

        Vector2 direction = (Vector2)RuntimeContext.PlayerCollider.bounds.center - origin;
        if (direction.sqrMagnitude <= Mathf.Epsilon) direction = areaAttackOrigin.right;

        DamageInfo damageInfo = new(
            ScaleDamage(areaAttackDamage),
            gameObject,
            hitPoint,
            direction.normalized);
        RuntimeContext.PlayerHealth.TryTakeDamage(damageInfo);
    }

    private void GenerateSplitAttackCenters()
    {
        Vector2 playerRootPosition = RuntimeContext.Player.position;
        Vector2 colliderCenterOffset =
            (Vector2)RuntimeContext.PlayerCollider.bounds.center - playerRootPosition;
        Vector2 minimumRootPosition = RuntimeContext.CombatBounds.Clamp(
            playerRootPosition,
            new Vector2(float.MinValue, float.MinValue),
            RuntimeContext.PlayerCollider);
        Vector2 maximumRootPosition = RuntimeContext.CombatBounds.Clamp(
            playerRootPosition,
            new Vector2(float.MaxValue, float.MaxValue),
            RuntimeContext.PlayerCollider);
        Vector2 minimumCenter = minimumRootPosition + colliderCenterOffset;
        Vector2 maximumCenter = maximumRootPosition + colliderCenterOffset;
        int attackCount = Random.Range(
            minimumSplitAreaAttackCount,
            maximumSplitAreaAttackCount + 1);

        splitAttackCenters.Clear();

        for (int index = 0; index < attackCount; index++)
        {
            splitAttackCenters.Add(new Vector2(
                Random.Range(minimumCenter.x, maximumCenter.x),
                Random.Range(minimumCenter.y, maximumCenter.y)));
        }
    }

    private void ApplySplitAreaAttack()
    {
        foreach (Vector2 center in splitAttackCenters)
        {
            Vector2 hitPoint = RuntimeContext.PlayerCollider.ClosestPoint(center);
            if ((hitPoint - center).sqrMagnitude > splitAreaAttackRadius * splitAreaAttackRadius)
                continue;

            Vector2 direction = (Vector2)RuntimeContext.PlayerCollider.bounds.center - center;
            if (direction.sqrMagnitude <= Mathf.Epsilon) direction = transform.up;

            DamageInfo damageInfo = new(
                ScaleDamage(splitAreaAttackDamage),
                gameObject,
                hitPoint,
                direction.normalized);
            RuntimeContext.PlayerHealth.TryTakeDamage(damageInfo);
            return;
        }
    }

    private void CompleteSpinningAttack()
    {
        EndSpinningProjectile();
        BeginRecovery(spinningAttackRecoveryDuration);
    }

    private void UpdateCloseDash()
    {
        float remainingDistance = maximumCloseAttackDistance - travelledDashDistance;

        if (remainingDistance <= Mathf.Epsilon)
        {
            CompleteCloseAttack();
            return;
        }

        float requestedDistance = Mathf.Min(
            closeAttackSpeed * MovementSpeedMultiplier * Time.fixedDeltaTime,
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

        bool hitPlayer = TryDamagePlayerWithDash(movementDistance, out float playerHitDistance);
        if (!isRunning) return;

        if (hitPlayer)
        {
            float stoppedDistance = Mathf.Min(movementDistance, playerHitDistance);
            if (stoppedDistance > Mathf.Epsilon)
                body.MovePosition(currentPosition + dashDirection * stoppedDistance);

            travelledDashDistance += stoppedDistance;
            CompleteCloseAttack();
            return;
        }

        if (movementDistance > Mathf.Epsilon) body.MovePosition(clampedPosition);

        travelledDashDistance += movementDistance;
        if (reachedCombatBounds || maximumCloseAttackDistance - travelledDashDistance <= Mathf.Epsilon)
            CompleteCloseAttack();
    }

    private void CompleteCloseAttack()
    {
        RestorePlayerCollision();
        body.linearVelocity = Vector2.zero;
        BeginRecovery(closeAttackRecoveryDuration);
    }

    private bool TryDamagePlayerWithDash(float castDistance, out float hitDistance)
    {
        hitDistance = 0f;
        if (hasHitPlayerDuringDash) return false;

        Physics2D.IgnoreCollision(bodyCollider, RuntimeContext.PlayerCollider, false);
        bool hasHit = TryGetPlayerDashHitPoint(
            castDistance,
            out Vector2 hitPoint,
            out hitDistance);
        Physics2D.IgnoreCollision(bodyCollider, RuntimeContext.PlayerCollider, true);

        if (!hasHit) return false;

        hasHitPlayerDuringDash = true;
        DamageInfo damageInfo = new(ScaleDamage(closeAttackDamage), gameObject, hitPoint, dashDirection);
        RuntimeContext.PlayerHealth.TryTakeDamage(damageInfo);
        return true;
    }

    private bool TryGetPlayerDashHitPoint(
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

    private void BeginRecovery(float duration)
    {
        phase = Phase.Recovering;
        remainingPhaseTime = duration;

        if (remainingPhaseTime <= 0f) BeginWaiting(true);
    }

    private void BeginWaiting(bool includeTransitionDelay)
    {
        phase = Phase.Waiting;
        remainingPhaseTime = Random.Range(minimumPatternInterval, maximumPatternInterval);
        if (includeTransitionDelay) remainingPhaseTime += TransitionDelay;

        PlayAnimation(idleAnimationStateName);
    }

    private void StartBehavior()
    {
        isRunning = true;
        BeginWaiting(false);
    }

    private void StopBehavior()
    {
        isRunning = false;
        phase = Phase.Waiting;
        RestorePlayerCollision();
        EndSpinningProjectile();
        splitAttackCenters.Clear();
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

    private void EndSpinningProjectile()
    {
        if (activeSpinningProjectile == null) return;

        Destroy(activeSpinningProjectile.gameObject);
        activeSpinningProjectile = null;
    }

    private void PlayAnimation(string stateName)
    {
        if (string.IsNullOrWhiteSpace(stateName)) return;

        for (int layerIndex = 0; layerIndex < animator.layerCount; layerIndex++)
        {
            if (TryPlayAnimation(stateName, layerIndex)) return;

            string fullPath = $"{animator.GetLayerName(layerIndex)}.{stateName}";
            if (TryPlayAnimation(fullPath, layerIndex)) return;
        }
    }

    private bool TryPlayAnimation(string stateName, int layerIndex)
    {
        int stateHash = Animator.StringToHash(stateName);
        if (!animator.HasState(layerIndex, stateHash)) return false;

        animator.Play(stateHash, layerIndex, 0f);
        return true;
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

        StopBehavior();
        RuntimeContext.CombatBridge.PlayerDied -= HandlePlayerDied;
    }

    private void OnValidate()
    {
        maximumPatternInterval = Mathf.Max(minimumPatternInterval, maximumPatternInterval);
        maximumSplitAreaAttackCount = Mathf.Max(
            minimumSplitAreaAttackCount,
            maximumSplitAreaAttackCount);
    }

    private void OnDrawGizmosSelected()
    {
        if (areaAttackOrigin != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(areaAttackOrigin.position, areaAttackRadius);
        }

        Gizmos.color = Color.yellow;

        foreach (Vector2 center in splitAttackCenters)
            Gizmos.DrawWireSphere(center, splitAreaAttackRadius);
    }

    private void Reset()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        bodyCollider = GetComponent<Collider2D>();
        areaAttackOrigin = transform;
        spinningAttackOrigin = transform;
    }

    private int ScaleDamage(int baseDamage) =>
        EnemyDifficultyUtility.ScaleStat(baseDamage, difficultyFactor);

    private void HandlePlayerDied() => StopBehavior();
}
