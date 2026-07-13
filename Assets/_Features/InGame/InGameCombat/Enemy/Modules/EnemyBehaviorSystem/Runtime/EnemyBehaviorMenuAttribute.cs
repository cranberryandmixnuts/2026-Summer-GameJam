using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class EnemyBehaviorMenuAttribute : Attribute
{
    public string Path { get; }

    public EnemyBehaviorMenuAttribute(string path) => Path = path;
}
