using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public sealed class ArmedSpadeSoldier : MonoBehaviour, IEnemyRuntimeInitializable, IEnemyDifficultyInitializable
{
    private const int BurstCount = 2;
    private const int ProjectileCountPerBurst = 3;

    private static readonly int MoveAnimationHash = Animator.StringToHash("Move");
    private static readonly int ReadyAnimationHash = Animator.StringToHash("ready");
    private static readonly int AttackAnimationHash = Animator.StringToHash("attack");

    private enum Phase
    {
        Moving,
        PreparingAttack,
        Bursting,
        Recovering
    }

    [TitleGroup("참조")]
    [SerializeField, Required] private Rigidbody2D body;
    [TitleGroup("참조")]
    [SerializeField, Required] private Collider2D detectionCollider;
    [TitleGroup("참조")]
    [SerializeField, Required] private Animator animator;
    [TitleGroup("참조")]
    [SerializeField, Required] private Transform muzzle;
    [TitleGroup("참조")]
    [SerializeField, Required] private EnemyProjectile projectilePrefab;

    [TitleGroup("이동")]
    [SerializeField, MinValue(0f)] private float downwardMovementSpeed = 1f;
    [TitleGroup("이동")]
    [SerializeField, MinValue(0f)] private float retreatMovementSpeed = 1f;

    [TitleGroup("2점사 산탄")]
    [SerializeField, MinValue(1)] private int projectileDamage = 1;
    [TitleGroup("2점사 산탄")]
    [SerializeField, MinValue(0.01f)] private float projectileSpeed = 6f;
    [TitleGroup("2점사 산탄")]
    [SerializeField, MinValue(0f), SuffixLabel("°")] private float spreadAngle = 30f;
    [TitleGroup("2점사 산탄")]
    [SerializeField, MinValue(0.01f), SuffixLabel("초")] private float burstInterval = 0.1f;
    [TitleGroup("2점사 산탄")]
    [SerializeField, MinValue(0.01f), HorizontalGroup("2점사 산탄/발사 간격"), LabelText("최소")]
    private float minimumShotInterval = 3f;
    [TitleGroup("2점사 산탄")]
    [SerializeField, MinValue(0.01f), HorizontalGroup("2점사 산탄/발사 간격"), LabelText("최대")]
    private float maximumShotInterval = 6f;
    [TitleGroup("2점사 산탄")]
    [SerializeField, MinValue(0f), SuffixLabel("초")] private float attackPreparationDuration = 0.5f;
    [TitleGroup("2점사 산탄")]
    [SerializeField, MinValue(0f), SuffixLabel("초")] private float recoveryDuration = 0.2f;

    [TitleGroup("애니메이션")]
    [SerializeField, MinValue(0f), SuffixLabel("초")] private float animationTransitionDuration = 0.08f;

    private Phase phase;
    private float remainingPhaseTime;
    private float remainingBurstCooldown;
    private float transitionDelay;
    private float difficultyFactor = 1f;
    private float movementSpeedMultiplier = 1f;
    private int remainingBurstCount;
    private int currentAnimationHash;
    private bool isRetreating;
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
        if (!isRunning) return;

        switch (phase)
        {
            case Phase.Moving:
            case Phase.PreparingAttack:
            case Phase.Recovering:
                UpdateTimedPhase();
                break;
            case Phase.Bursting:
                UpdateBurst();
                break;
        }
    }

    private void FixedUpdate()
    {
        if (!isRunning || phase != Phase.Moving) return;

        isRetreating = detectionCollider.Distance(RuntimeContext.PlayerCollider).isOverlapped;

        Vector2 direction = isRetreating ? Vector2.up : Vector2.down;
        float speed = isRetreating ? retreatMovementSpeed : downwardMovementSpeed;

        body.MovePosition(
            body.position + direction * speed * MovementSpeedMultiplier * Time.fixedDeltaTime);
    }

    private void UpdateTimedPhase()
    {
        remainingPhaseTime -= Time.deltaTime;
        if (remainingPhaseTime > 0f) return;

        switch (phase)
        {
            case Phase.Moving:
                BeginAttackPreparation();
                break;
            case Phase.PreparingAttack:
                TryBeginBurstAttack();
                break;
            case Phase.Recovering:
                BeginMovement(true);
                break;
        }
    }

    private void UpdateBurst()
    {
        remainingBurstCooldown -= Time.deltaTime;
        if (remainingBurstCooldown > 0f) return;
        if (!TryGetAimDirection(out Vector2 aimDirection)) return;

        FireSpread(aimDirection);
        remainingBurstCount--;

        if (remainingBurstCount > 0)
        {
            remainingBurstCooldown = burstInterval;
            return;
        }

        BeginRecovery();
    }

    private void BeginMovement(bool includeTransitionDelay)
    {
        phase = Phase.Moving;
        remainingPhaseTime = GetRandomShotInterval();
        if (includeTransitionDelay) remainingPhaseTime += TransitionDelay;

        remainingBurstCount = 0;
        remainingBurstCooldown = 0f;
        body.linearVelocity = Vector2.zero;
        PlayAnimation(MoveAnimationHash);
    }

    private void BeginAttackPreparation()
    {
        phase = Phase.PreparingAttack;
        remainingPhaseTime = attackPreparationDuration;
        body.linearVelocity = Vector2.zero;
        PlayAnimation(ReadyAnimationHash);

        if (remainingPhaseTime <= 0f) TryBeginBurstAttack();
    }

    private void TryBeginBurstAttack()
    {
        if (!TryGetAimDirection(out Vector2 aimDirection)) return;

        phase = Phase.Bursting;
        remainingBurstCount = BurstCount - 1;
        remainingBurstCooldown = burstInterval;
        body.linearVelocity = Vector2.zero;
        PlayAnimation(AttackAnimationHash);
        FireSpread(aimDirection);

        if (remainingBurstCount <= 0) BeginRecovery();
    }

    private void BeginRecovery()
    {
        phase = Phase.Recovering;
        remainingPhaseTime = recoveryDuration;
        body.linearVelocity = Vector2.zero;
        PlayAnimation(AttackAnimationHash);

        if (remainingPhaseTime <= 0f) BeginMovement(true);
    }

    private bool TryGetAimDirection(out Vector2 aimDirection)
    {
        aimDirection = RuntimeContext.Player.position - muzzle.position;
        if (aimDirection.sqrMagnitude <= Mathf.Epsilon) return false;

        aimDirection.Normalize();
        return true;
    }

    private void FireSpread(Vector2 aimDirection)
    {
        float startAngle = -spreadAngle * 0.5f;
        float angleStep = spreadAngle / (ProjectileCountPerBurst - 1);

        for (int index = 0; index < ProjectileCountPerBurst; index++)
        {
            float angleOffset = startAngle + angleStep * index;
            Vector2 direction = Quaternion.Euler(0f, 0f, angleOffset) * aimDirection;
            FireProjectile(direction);
        }
    }

    private void FireProjectile(Vector2 direction)
    {
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
        BeginMovement(false);
    }

    private void StopBehavior()
    {
        isRunning = false;
        phase = Phase.Moving;
        remainingBurstCount = 0;
        remainingBurstCooldown = 0f;
        currentAnimationHash = 0;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
    }

    private float GetRandomShotInterval() =>
        Random.Range(minimumShotInterval, maximumShotInterval);

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
        maximumShotInterval = Mathf.Max(minimumShotInterval, maximumShotInterval);

    private void Reset()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        muzzle = transform;
    }

    private int ScaleDamage(int baseDamage) =>
        EnemyDifficultyUtility.ScaleStat(baseDamage, difficultyFactor);

    private void HandlePlayerDied() => StopBehavior();
}