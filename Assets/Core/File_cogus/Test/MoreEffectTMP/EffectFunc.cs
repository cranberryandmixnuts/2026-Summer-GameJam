using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public partial class MoreEffectTMP
{
    private static readonly IReadOnlyDictionary<TMP_EffectType, Func<float, int, float, (Vector3, Vector3)>> _changePosFunc =
        new Dictionary<TMP_EffectType, Func<float, int, float, (Vector3, Vector3)>>
        {
            { TMP_EffectType.Flow, Flow },
            { TMP_EffectType.Shake, Random }
        };

    private static (Vector3, Vector3) Flow(float pTimer, int pIdx, float pArg) =>
        ((Mathf.Sin(pIdx * 0.3f + pTimer * Mathf.PI * pArg * 2) + 1) / 2 * Vector3.up, Vector3.zero);

    private static (Vector3, Vector3) Random(float pTimer, int pIdx, float pArg)
    {
        Random random = new(pIdx + Mathf.FloorToInt(pTimer * 30f / pArg));
        return (new Vector3(((float)random.NextDouble() - 0.5f) * 2, (float)random.NextDouble() * 0.5f), Vector3.zero);
    }
}
