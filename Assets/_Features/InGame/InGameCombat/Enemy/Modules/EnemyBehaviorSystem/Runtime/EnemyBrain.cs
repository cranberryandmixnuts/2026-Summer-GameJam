using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class EnemyBrain : MonoBehaviour, IEnemyRuntimeInitializable
{
    [SerializeField, Required] private Rigidbody2D body;
    [SerializeField] private MonoBehaviour healthSource;
    [SerializeField, HideInInspector] private EnemyBehaviorGraph graph = new();

    private EnemyBehaviorContext behaviorContext;
    private EnemyState currentState;
    private int sequenceIndex;
    private bool isInitialized;
    private bool isRunning;

    public Rigidbody2D Body => body;
    public IEnemyHealthSource Health => (IEnemyHealthSource)healthSource;
    public EnemyRuntimeContext RuntimeContext { get; private set; }
    public EnemyState CurrentState => currentState;
    public float StateElapsedTime { get; private set; }
    public bool ActionsComplete => GetActionsComplete();

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
        if (!isRunning || currentState == null) return;

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
        currentState = state;
        StateElapsedTime = 0f;
        sequenceIndex = 0;

        if (currentState == null) return;

        if (currentState.ExecutionMode == EnemyActionExecutionMode.Parallel)
        {
            foreach (EnemyAction action in currentState.Actions)
                action.Enter(behaviorContext);

            return;
        }

        if (currentState.Actions.Count > 0)
            currentState.Actions[0].Enter(behaviorContext);
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
            currentState.Actions[sequenceIndex].Enter(behaviorContext);
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

            ExitState();
            EnterState(targetState);
            return true;
        }

        return false;
    }

    private void Stop()
    {
        if (isRunning) ExitState();

        isRunning = false;
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
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
    }

    private void OnDestroy()
    {
        if (!isInitialized) return;

        RuntimeContext.CombatBridge.PlayerDied -= HandlePlayerDied;
    }

    private void OnValidate() => graph.EnsureStateIds();

    private void Reset() => body = GetComponent<Rigidbody2D>();

    private void HandlePlayerDied() => Stop();
}