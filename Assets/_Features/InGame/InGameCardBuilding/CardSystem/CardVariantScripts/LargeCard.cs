using Sirenix.OdinInspector;
using UnityEngine;

public sealed class LargeCard : Card
{
    [SerializeField, MinValue(0f), SuffixLabel("%")] private float sizeChangePercent = 50f;

    private void Awake() =>
        AddEffect(new CardEffect
        {
            SizeChangePercent = sizeChangePercent
        });
}
