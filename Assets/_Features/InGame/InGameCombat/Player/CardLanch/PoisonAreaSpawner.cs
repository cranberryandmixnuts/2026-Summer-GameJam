using Sirenix.OdinInspector;
using UnityEngine;

public sealed class PoisonAreaSpawner : BaseBehaviour
{
    [SerializeField, Required, AssetsOnly] private PoisonArea prefab;

    public void Spawn(
        Vector3 position,
        CardProjectileSettings settings,
        LayerMask enemyLayers,
        GameObject source,
        int finalProjectileDamage
    )
    {
        PoisonArea area = Instantiate(prefab);
        area.Activate(
            position,
            settings,
            enemyLayers,
            source,
            finalProjectileDamage
        );
    }
}
