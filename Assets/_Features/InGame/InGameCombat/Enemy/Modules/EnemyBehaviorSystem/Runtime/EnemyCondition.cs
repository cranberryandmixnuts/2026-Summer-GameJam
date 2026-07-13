using System;

[Serializable]
public abstract class EnemyCondition
{
    public abstract bool Evaluate(in EnemyBehaviorContext context);
}
