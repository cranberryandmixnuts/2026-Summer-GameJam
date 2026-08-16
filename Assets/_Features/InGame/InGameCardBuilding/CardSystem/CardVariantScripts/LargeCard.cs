using Sirenix.OdinInspector;
using UnityEngine;

public sealed class LargeCard : Card
{
    [SerializeField, MinValue(1f)] private float sizeMultiplier = 1.5f;

    private void Awake() =>
        AddEffect(new CardEffect
        {
            SizeMultiplier = sizeMultiplier
        });
}
