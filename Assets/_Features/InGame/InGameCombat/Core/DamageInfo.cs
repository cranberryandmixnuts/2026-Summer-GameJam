using UnityEngine;

public readonly struct DamageInfo
{
    public int Amount { get; }
    public GameObject Source { get; }
    public Vector2 HitPoint { get; }
    public Vector2 Direction { get; }

    public DamageInfo(int amount, GameObject source, Vector2 hitPoint, Vector2 direction)
    {
        Amount = amount;
        Source = source;
        HitPoint = hitPoint;
        Direction = direction;
    }
}
