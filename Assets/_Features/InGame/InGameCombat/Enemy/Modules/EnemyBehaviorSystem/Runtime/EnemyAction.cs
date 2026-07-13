using System;

[Serializable]
public abstract class EnemyAction
{
    public virtual void Enter(in EnemyBehaviorContext context) { }

    public virtual void Update(in EnemyBehaviorContext context) { }

    public virtual void FixedUpdate(in EnemyBehaviorContext context) { }

    public virtual void Exit(in EnemyBehaviorContext context) { }

    public virtual bool IsComplete(in EnemyBehaviorContext context) => false;
}
