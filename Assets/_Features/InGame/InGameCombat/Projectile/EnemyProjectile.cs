using System;
using Sirenix.OdinInspector;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class EnemyProjectile : MonoBehaviour
{
    [SerializeField, Required] private Rigidbody2D body;
    [SerializeField] private LayerMask targetLayers;

    private Action<EnemyProjectile> release;
    private GameObject source;
    private Vector2 direction;
    private float remainingLifetime;
    private int damage;
    private bool isLaunched;

    public void Initialize(Action<EnemyProjectile> releaseAction) => release = releaseAction;

    public void Launch(
        Vector2 position,
        Vector2 launchDirection,
        float speed,
        int damageAmount,
        float lifetime,
        GameObject damageSource)
    {
        direction = launchDirection;
        damage = damageAmount;
        remainingLifetime = lifetime;
        source = damageSource;
        isLaunched = true;

        body.position = position;
        body.rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        body.linearVelocity = direction * speed;
    }

    private void Update()
    {
        if (!isLaunched) return;

        remainingLifetime -= Time.deltaTime;
        if (remainingLifetime <= 0f) Despawn();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isLaunched || !targetLayers.Contains(other.gameObject.layer)) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null) return;

        Vector2 hitPoint = other.ClosestPoint(body.position);
        DamageInfo damageInfo = new(damage, source, hitPoint, direction);

        damageable.TryTakeDamage(damageInfo);
        Despawn();
    }

    private void Despawn()
    {
        if (!isLaunched) return;

        isLaunched = false;
        body.linearVelocity = Vector2.zero;
        release(this);
    }

    private void OnDisable()
    {
        isLaunched = false;
        body.linearVelocity = Vector2.zero;
        source = null;
    }

    private void Reset() => body = GetComponent<Rigidbody2D>();
}
