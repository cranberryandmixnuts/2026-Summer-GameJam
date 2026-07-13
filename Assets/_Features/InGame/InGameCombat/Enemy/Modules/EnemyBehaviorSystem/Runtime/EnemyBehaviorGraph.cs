using System;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyActionExecutionMode
{
    [InspectorName("병렬")]
    Parallel,

    [InspectorName("순서")]
    Sequence
}

public enum EnemyConditionEvaluationMode
{
    [InspectorName("모두 만족")]
    All,

    [InspectorName("하나 이상 만족")]
    Any
}

[Serializable]
public sealed class EnemyConditionSlot
{
    [SerializeField] private bool inverted;
    [SerializeReference] private EnemyCondition condition;

    public bool Inverted => inverted;
    public EnemyCondition Condition => condition;

    public bool Evaluate(in EnemyBehaviorContext context)
    {
        bool result = condition.Evaluate(context);
        return inverted ? !result : result;
    }
}

[Serializable]
public sealed class EnemyTransition
{
    [SerializeField, HideInInspector] private string targetStateId;
    [SerializeField, Min(0f)] private float minimumStateDuration;
    [SerializeField] private bool allowSelfTransition;
    [SerializeField] private EnemyConditionEvaluationMode evaluationMode;
    [SerializeField] private List<EnemyConditionSlot> conditions = new();

    public string TargetStateId => targetStateId;
    public float MinimumStateDuration => minimumStateDuration;
    public bool AllowSelfTransition => allowSelfTransition;
    public IReadOnlyList<EnemyConditionSlot> Conditions => conditions;

    public bool Evaluate(in EnemyBehaviorContext context)
    {
        if (context.StateElapsedTime < minimumStateDuration) return false;
        if (conditions.Count == 0) return true;

        if (evaluationMode == EnemyConditionEvaluationMode.All)
        {
            foreach (EnemyConditionSlot condition in conditions)
            {
                if (!condition.Evaluate(context)) return false;
            }

            return true;
        }

        foreach (EnemyConditionSlot condition in conditions)
        {
            if (condition.Evaluate(context)) return true;
        }

        return false;
    }
}

[Serializable]
public sealed class EnemyState
{
    [SerializeField, HideInInspector] private string id;
    [SerializeField] private string name = "New State";
    [SerializeField, HideInInspector] private Vector2 editorPosition;
    [SerializeField] private EnemyActionExecutionMode executionMode;
    [SerializeField] private bool loopSequence;
    [SerializeReference] private List<EnemyAction> actions = new();
    [SerializeField] private List<EnemyTransition> transitions = new();

    public string Id => id;
    public string Name => name;
    public Vector2 EditorPosition => editorPosition;
    public EnemyActionExecutionMode ExecutionMode => executionMode;
    public bool LoopSequence => loopSequence;
    public IReadOnlyList<EnemyAction> Actions => actions;
    public IReadOnlyList<EnemyTransition> Transitions => transitions;

    public static EnemyState Create(string stateName, Vector2 position) => new()
    {
        id = Guid.NewGuid().ToString("N"),
        name = stateName,
        editorPosition = position
    };

    public void EnsureId()
    {
        if (string.IsNullOrWhiteSpace(id)) id = Guid.NewGuid().ToString("N");
    }
}

[Serializable]
public sealed class EnemyBehaviorGraph
{
    [SerializeField, HideInInspector] private string entryStateId;
    [SerializeField] private List<EnemyTransition> globalTransitions = new();
    [SerializeField] private List<EnemyState> states = new();

    public string EntryStateId => entryStateId;
    public IReadOnlyList<EnemyTransition> GlobalTransitions => globalTransitions;
    public IReadOnlyList<EnemyState> States => states;

    public EnemyState FindState(string stateId)
    {
        foreach (EnemyState state in states)
        {
            if (state.Id == stateId) return state;
        }

        return null;
    }

    public EnemyState GetEntryState() => FindState(entryStateId);

    public void EnsureStateIds()
    {
        foreach (EnemyState state in states) state.EnsureId();

        if (states.Count > 0 && FindState(entryStateId) == null) entryStateId = states[0].Id;
    }
}
