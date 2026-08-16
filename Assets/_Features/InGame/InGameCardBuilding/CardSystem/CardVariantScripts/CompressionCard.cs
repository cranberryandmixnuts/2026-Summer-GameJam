using Sirenix.OdinInspector;
using UnityEngine;

public sealed class CompressionCard : Card
{
    [SerializeField, MinValue(0.01f), MaxValue(1f)] private float sizeMultiplier = 0.5f;

    private void Awake() =>
        AddEffect(new CardEffect
        {
            SizeMultiplier = sizeMultiplier
        });
}
