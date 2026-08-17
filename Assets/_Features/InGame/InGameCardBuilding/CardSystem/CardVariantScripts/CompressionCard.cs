using Sirenix.OdinInspector;
using UnityEngine;

public sealed class CompressionCard : Card
{
    [SerializeField, MinValue(-100f), MaxValue(0f), SuffixLabel("%")] private float sizeChangePercent = -50f;

    private void Awake() =>
        AddEffect(new CardEffect
        {
            SizeChangePercent = sizeChangePercent
        });
}
