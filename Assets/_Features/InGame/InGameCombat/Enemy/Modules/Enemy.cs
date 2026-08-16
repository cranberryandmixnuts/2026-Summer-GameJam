using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public abstract class Enemy : MonoBehaviour, IEnemyRuntimeInitializable, IEnemyDifficultyInitializable
{
    [TitleGroup("공통 참조")]
    [SerializeField, Required] private Rigidbody2D body;
    [TitleGroup("공통 참조")]
    [SerializeField, Required] private Animator animator;

    private float transitionDelay;
    private float movementSpeedMultiplier = 1f;
    private float difficultyFactor = 1f;
    private bool isInitialized;

    public EnemyRuntimeContext RuntimeContext { get; private set; }
    public float DifficultyFactor => difficultyFactor;
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

    protected Rigidbody2D Body => body;
    protected bool IsRunning { get; private set; }

    public void InitializeDifficulty(float value) =>
        difficultyFactor = EnemyDifficultyUtility.ClampFactor(value);

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

    protected abstract void OnBehaviorStarted();

    protected virtual void OnBehaviorStopped() { }

    protected virtual void OnReset() { }

    protected int ScaleDamage(int baseDamage) =>
        EnemyDifficultyUtility.ScaleStat(baseDamage, difficultyFactor);

    protected void PlayAnimation(string stateName)
    {
        if (string.IsNullOrWhiteSpace(stateName)) return;

        for (int layerIndex = 0; layerIndex < animator.layerCount; layerIndex++)
        {
            if (TryPlayAnimation(stateName, layerIndex)) return;

            string fullPath = $"{animator.GetLayerName(layerIndex)}.{stateName}";
            if (TryPlayAnimation(fullPath, layerIndex)) return;
        }
    }

    protected virtual void OnEnable()
    {
        if (!isInitialized || IsRunning) return;

        StartBehavior();
    }

    protected virtual void OnDisable() => StopBehavior();

    protected virtual void OnDestroy()
    {
        if (!isInitialized) return;

        StopBehavior();
        RuntimeContext.CombatBridge.PlayerDied -= HandlePlayerDied;
    }

    protected virtual void Reset()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        OnReset();
    }

    private void StartBehavior()
    {
        IsRunning = true;
        OnBehaviorStarted();
    }

    private void StopBehavior()
    {
        if (!IsRunning) return;

        IsRunning = false;
        OnBehaviorStopped();
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
    }

    private bool TryPlayAnimation(string stateName, int layerIndex)
    {
        int stateHash = Animator.StringToHash(stateName);
        if (!animator.HasState(layerIndex, stateHash)) return false;

        animator.Play(stateHash, layerIndex, 0f);
        return true;
    }

    private void HandlePlayerDied() => StopBehavior();
}
