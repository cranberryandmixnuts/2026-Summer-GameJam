using UnityEngine;

public static class EnemyCollisionResolver
{
    public static bool TryResolve(
        Collider2D other,
        LayerMask enemyLayers,
        out EnemyHealth enemyHealth
    )
    {
        if (!other.TryGetComponent(out enemyHealth)) enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth == null) return false;

        return enemyLayers.Contains(other.gameObject.layer)
            || enemyLayers.Contains(enemyHealth.gameObject.layer);
    }
}
