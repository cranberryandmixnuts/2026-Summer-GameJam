using System;
using UnityEngine;

[Serializable]
public abstract class EnemyAction
{
    [SerializeField, EnemyBehaviorField("재생 애니메이션")] private string animationStateName;

    public string AnimationStateName => animationStateName;

    public virtual void Enter(in EnemyBehaviorContext context) { }

    public virtual void Update(in EnemyBehaviorContext context) { }

    public virtual void FixedUpdate(in EnemyBehaviorContext context) { }

    public virtual void Exit(in EnemyBehaviorContext context) { }

    public virtual bool IsComplete(in EnemyBehaviorContext context) => false;
}