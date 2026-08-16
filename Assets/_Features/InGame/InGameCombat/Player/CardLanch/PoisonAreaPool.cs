using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public sealed class PoisonAreaPool : MonoBehaviour
{
    [SerializeField, Required, AssetsOnly] private PoisonArea prefab;
    [SerializeField, Required] private Transform poolRoot;
    [SerializeField, MinValue(0)] private int preloadCount = 8;

    private readonly Stack<PoisonArea> availableAreas = new();

    private void Awake()
    {
        for (int index = 0; index < preloadCount; index++) availableAreas.Push(CreateArea());
    }

    public void Spawn(
        Vector3 position,
        CardProjectileSettings settings,
        LayerMask enemyLayers,
        GameObject source,
        int finalProjectileDamage
    )
    {
        PoisonArea area = availableAreas.Count > 0
            ? availableAreas.Pop()
            : CreateArea();

        area.transform.SetParent(null, true);
        area.Activate(
            position,
            settings,
            enemyLayers,
            source,
            finalProjectileDamage,
            Release
        );
    }

    private PoisonArea CreateArea()
    {
        PoisonArea area = Instantiate(prefab, poolRoot);
        area.gameObject.SetActive(false);
        return area;
    }

    private void Release(PoisonArea area)
    {
        area.gameObject.SetActive(false);
        area.transform.SetParent(poolRoot, false);
        availableAreas.Push(area);
    }

    private void Reset() => poolRoot = transform;
}
