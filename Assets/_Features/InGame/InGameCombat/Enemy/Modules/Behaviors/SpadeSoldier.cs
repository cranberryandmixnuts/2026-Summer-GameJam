using Sirenix.OdinInspector;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public sealed class SpadeSoldier : MonoBehaviour, IEnemyRuntimeInitializable
{
    private const int ProjectileCount = 3;

    [TitleGroup("참조")]
    [SerializeField, Required] private Rigidbody2D body;
    [TitleGroup("참조")]
    [SerializeField, Required] private Transform muzzle;
    [TitleGroup("참조")]
    [SerializeField, Required] private EnemyProjectile projectilePrefab;

    [TitleGroup("이동")]
    [SerializeField, MinValue(0f)] private float movementSpeed = 1f;

    [TitleGroup("공격")]
    [SerializeField, MinValue(1)] private int projectileDamage = 1;
    [TitleGroup("공격")]
    [SerializeField, MinValue(0.01f)] private float projectileSpeed = 6f;
    [TitleGroup("공격")]
    [SerializeField, MinValue(0f), SuffixLabel("°")] private float spreadAngle = 30f;
    [TitleGroup("공격")]
    [SerializeField, MinValue(0.01f), HorizontalGroup("공격/발사 간격"), LabelText("최소")]
    private float minimumShotInterval = 3f;
    [TitleGroup("공격")]
    [SerializeField, MinValue(0.01f), HorizontalGroup("공격/발사 간격"), LabelText("최대")]
    private float maximumShotInterval = 6f;

    private float remainingShotCooldown;
    private float transitionDelay;
    private float movementSpeedMultiplier = 1f;
    private bool isInitialized;
    private bool isRunning;

    public EnemyRuntimeContext RuntimeContext { get; private set; }
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

        remainingShotCooldown -= Time.deltaTime;
        if (remainingShotCooldown > 0f) return;
        if (!TryFireSpread()) return;

        remainingShotCooldown = GetRandomShotInterval() + TransitionDelay;
    }

    private void FixedUpdate()
    {
        if (!isRunning) return;

        body.MovePosition(
            body.position + Vector2.down * movementSpeed * MovementSpeedMultiplier * Time.fixedDeltaTime);
    }

    private bool TryFireSpread()
    {
        Vector2 aimDirection = RuntimeContext.Player.position - muzzle.position;
        if (aimDirection.sqrMagnitude <= Mathf.Epsilon) return false;

        aimDirection.Normalize();

        float startAngle = -spreadAngle * 0.5f;
        float angleStep = spreadAngle / (ProjectileCount - 1);

        for (int index = 0; index < ProjectileCount; index++)
        {
            float angleOffset = startAngle + angleStep * index;
            Vector2 direction = Quaternion.Euler(0f, 0f, angleOffset) * aimDirection;
            FireProjectile(direction);
        }

        return true;
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
            projectileDamage,
            gameObject,
            RuntimeContext,
            RuntimeContext.Player);
    }

    private void StartBehavior()
    {
        isRunning = true;
        remainingShotCooldown = GetRandomShotInterval();
    }

    private void StopBehavior()
    {
        isRunning = false;
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
        muzzle = transform;
    }

    private void HandlePlayerDied() => StopBehavior();
}
