using UnityEngine;

public sealed class EnemyBehaviorFieldAttribute : PropertyAttribute
{
    public string Label { get; }
    public float Minimum { get; set; } = float.NegativeInfinity;
    public float Maximum { get; set; } = float.PositiveInfinity;

    public EnemyBehaviorFieldAttribute(string label) => Label = label;
}
