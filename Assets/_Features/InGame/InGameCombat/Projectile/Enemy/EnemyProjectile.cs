using UnityEngine;

public abstract class EnemyProjectile : MonoBehaviour
{
    private CombatBridge combatBridge;
    private GameObject source;
    private int damage;
    private bool isSubscribedToPlayerDied;

    protected EnemyRuntimeContext RuntimeContext { get; private set; }
    protected Transform Target { get; private set; }
    protected Vector2 Direction { get; set; }
    protected bool IsActive { get; private set; }

    public void Launch(
        Vector2 position,
        Vector2 direction,
        float speed,
        int damageAmount,
        GameObject damageSource,
        in EnemyRuntimeContext context) =>
        Launch(
            position,
            direction,
            speed,
            damageAmount,
            damageSource,
            context,
            null);

    public void Launch(
        Vector2 position,
        Vector2 direction,
        float speed,
        int damageAmount,
        GameObject damageSource,
        in EnemyRuntimeContext context,
        Transform target)
    {
        RuntimeContext = context;
        Target = target;
        combatBridge = context.CombatBridge;
        Direction = direction;
        damage = damageAmount;
        source = damageSource;
        IsActive = true;

        combatBridge.PlayerDied += HandlePlayerDied;
        isSubscribedToPlayerDied = true;

        OnLaunched(position, direction, speed);
    }

    protected abstract void OnLaunched(Vector2 position, Vector2 direction, float speed);

    protected abstract void StopMovement();

    public bool TryIntercept()
    {
        if (!IsActive) return false;

        DestroyProjectile();
        return true;
    }

    protected bool TryDamageTarget(Collider2D other, LayerMask targetLayers, Vector2 projectilePosition)
    {
        if (!IsActive || !targetLayers.Contains(other.gameObject.layer)) return false;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null) return false;

        Vector2 hitPoint = other.ClosestPoint(projectilePosition);
        DamageInfo damageInfo = new(damage, source, hitPoint, Direction);
        damageable.TryTakeDamage(damageInfo);
        return true;
    }

    protected void DestroyProjectile()
    {
        if (!IsActive) return;

        IsActive = false;
        UnsubscribeFromPlayerDied();
        Destroy(gameObject);
    }

    protected virtual void OnDestroy() => UnsubscribeFromPlayerDied();

    private void HandlePlayerDied()
    {
        if (!IsActive) return;

        IsActive = false;
        UnsubscribeFromPlayerDied();
        StopMovement();
    }

    private void UnsubscribeFromPlayerDied()
    {
        if (!isSubscribedToPlayerDied) return;

        combatBridge.PlayerDied -= HandlePlayerDied;
        isSubscribedToPlayerDied = false;
    }
}
