using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public sealed class EnemyBrain : MonoBehaviour, IEnemyRuntimeInitializable
{
    [SerializeField, Required] private Rigidbody2D body;
    [SerializeField, Required] private Animator animator;
    [SerializeField] private MonoBehaviour healthSource;
    [SerializeField, HideInInspector] private EnemyBehaviorGraph graph = new();

    private EnemyBehaviorContext behaviorContext;
    private EnemyState currentState;
    private EnemyState pendingState;
    private int sequenceIndex;
    private float transitionDelay = 0f;
    private float transitionDelayElapsedTime;
    private float movementSpeedMultiplier = 1f;
    private bool isInitialized;
    private bool isRunning;

    public Rigidbody2D Body => body;
    public Animator Animator => animator;
    public IEnemyHealthSource Health => (IEnemyHealthSource)healthSource;
    public EnemyRuntimeContext RuntimeContext { get; private set; }
    public EnemyState CurrentState => currentState;
    public float StateElapsedTime { get; private set; }
    public bool ActionsComplete => GetActionsComplete();
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
            if (isRunning) ExitState();

            RuntimeContext.CombatBridge.PlayerDied -= HandlePlayerDied;
        }

        RuntimeContext = context;
        behaviorContext = new EnemyBehaviorContext(this);
        graph.EnsureStateIds();
        isInitialized = true;
        isRunning = true;
        RuntimeContext.CombatBridge.PlayerDied += HandlePlayerDied;
        EnterState(graph.GetEntryState());
    }

    private void Update()
    {
        if (!isRunning) return;

        if (pendingState != null)
        {
            UpdateTransitionDelay();
            return;
        }

        if (currentState == null) return;

        StateElapsedTime += Time.deltaTime;
        UpdateActions();
        TryTransition();
    }

    private void FixedUpdate()
    {
        if (!isRunning || currentState == null) return;

        FixedUpdateActions();
    }

    private void EnterState(EnemyState state)
    {
        pendingState = null;
        transitionDelayElapsedTime = 0f;
        currentState = state;
        StateElapsedTime = 0f;
        sequenceIndex = 0;

        if (currentState == null) return;

        if (currentState.ExecutionMode == EnemyActionExecutionMode.Parallel)
        {
            foreach (EnemyAction action in currentState.Actions)
                EnterAction(action);

            return;
        }

        if (currentState.Actions.Count > 0)
            EnterAction(currentState.Actions[0]);
    }

    private void ExitState()
    {
        if (currentState == null) return;

        if (currentState.ExecutionMode == EnemyActionExecutionMode.Parallel)
        {
            foreach (EnemyAction action in currentState.Actions)
                action.Exit(behaviorContext);

            return;
        }

        if (sequenceIndex < currentState.Actions.Count)
            currentState.Actions[sequenceIndex].Exit(behaviorContext);
    }

    private void UpdateActions()
    {
        if (currentState.ExecutionMode == EnemyActionExecutionMode.Parallel)
        {
            foreach (EnemyAction action in currentState.Actions)
                action.Update(behaviorContext);

            return;
        }

        if (sequenceIndex >= currentState.Actions.Count) return;

        EnemyAction currentAction = currentState.Actions[sequenceIndex];
        currentAction.Update(behaviorContext);

        if (!currentAction.IsComplete(behaviorContext)) return;

        currentAction.Exit(behaviorContext);
        sequenceIndex++;

        if (sequenceIndex >= currentState.Actions.Count && currentState.LoopSequence)
            sequenceIndex = 0;

        if (sequenceIndex < currentState.Actions.Count)
            EnterAction(currentState.Actions[sequenceIndex]);
    }

    private void FixedUpdateActions()
    {
        if (currentState.ExecutionMode == EnemyActionExecutionMode.Parallel)
        {
            foreach (EnemyAction action in currentState.Actions)
                action.FixedUpdate(behaviorContext);

            return;
        }

        if (sequenceIndex < currentState.Actions.Count)
            currentState.Actions[sequenceIndex].FixedUpdate(behaviorContext);
    }

    private bool GetActionsComplete()
    {
        if (currentState == null) return true;

        if (currentState.ExecutionMode == EnemyActionExecutionMode.Sequence)
            return !currentState.LoopSequence && sequenceIndex >= currentState.Actions.Count;

        foreach (EnemyAction action in currentState.Actions)
        {
            if (!action.IsComplete(behaviorContext)) return false;
        }

        return true;
    }

    private void TryTransition()
    {
        if (TryTransition(graph.GlobalTransitions)) return;

        TryTransition(currentState.Transitions);
    }

    private bool TryTransition(IReadOnlyList<EnemyTransition> transitions)
    {
        foreach (EnemyTransition transition in transitions)
        {
            if (!transition.AllowSelfTransition && transition.TargetStateId == currentState.Id) continue;
            if (!transition.Evaluate(behaviorContext)) continue;

            EnemyState targetState = graph.FindState(transition.TargetStateId);
            if (targetState == null) continue;

            BeginTransition(targetState);
            return true;
        }

        return false;
    }

    private void BeginTransition(EnemyState targetState)
    {
        ExitState();
        currentState = null;

        if (transitionDelay <= 0f)
        {
            EnterState(targetState);
            return;
        }

        pendingState = targetState;
        transitionDelayElapsedTime = 0f;
    }

    private void UpdateTransitionDelay()
    {
        transitionDelayElapsedTime += Time.deltaTime;
        if (transitionDelayElapsedTime < transitionDelay) return;

        EnterState(pendingState);
    }

    private void EnterAction(EnemyAction action)
    {
        PlayAnimation(action.AnimationStateName);
        action.Enter(behaviorContext);
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
        int stateHash = UnityEngine.Animator.StringToHash(stateName);
        if (!animator.HasState(layerIndex, stateHash)) return false;

        animator.Play(stateHash, layerIndex, 0f);
        return true;
    }

    private void Stop()
    {
        if (isRunning) ExitState();

        isRunning = false;
        pendingState = null;
        transitionDelayElapsedTime = 0f;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
    }

    private void OnEnable()
    {
        if (!isInitialized || isRunning) return;

        isRunning = true;
        EnterState(graph.GetEntryState());
    }

    private void OnDisable()
    {
        if (isRunning) ExitState();

        isRunning = false;
        pendingState = null;
        transitionDelayElapsedTime = 0f;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
    }

    private void OnDestroy()
    {
        if (!isInitialized) return;

        RuntimeContext.CombatBridge.PlayerDied -= HandlePlayerDied;
    }

    private void OnValidate() => graph.EnsureStateIds();

    private void Reset()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void HandlePlayerDied() => Stop();
}
