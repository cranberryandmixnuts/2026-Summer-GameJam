using Sirenix.OdinInspector;
using UnityEngine;

public sealed class WallCard : Card
{
    [SerializeField, MinValue(0f)] private float knockbackDistance = 1f;

    private void Awake() =>
        AddEffect(new CardEffect
        {
            KnockbackDistance = knockbackDistance
        });
}
