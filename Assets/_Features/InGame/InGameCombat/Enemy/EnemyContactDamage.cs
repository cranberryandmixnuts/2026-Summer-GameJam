using Sirenix.OdinInspector;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyContactDamage : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayers;
    [SerializeField, MinValue(1)] private int damage = 1;

    private bool hasHit;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit || !targetLayers.Contains(other.gameObject.layer)) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null) return;

        hasHit = true;

        Vector2 hitPoint = other.ClosestPoint(transform.position);
        Vector2 direction = ((Vector2)other.transform.position - (Vector2)transform.position).normalized;
        DamageInfo damageInfo = new(damage, gameObject, hitPoint, direction);

        damageable.TryTakeDamage(damageInfo);
        Destroy(gameObject);
    }
}
