using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public sealed class CardProjectile : MonoBehaviour
{
    [SerializeField, Required] private Rigidbody2D body;
    [SerializeField, Required] private PoisonCardTrailEmitter poisonTrailEmitter;

    private readonly HashSet<EnemyHealth> hitEnemies = new();

    private CardEffect effect;
    private CardProjectileSettings settings;
    private LayerMask enemyLayers;
    private GameObject source;
    private int damage;
    private Vector2 direction;
    private bool isActive;

    public void Initialize(
        int finalDamage,
        float speed,
        CardEffect effect,
        CardProjectileSettings settings,
        PoisonAreaSpawner poisonAreaSpawner,
        LayerMask enemyLayers,
        GameObject source
    )
    {
        damage = finalDamage;
        this.effect = effect;
        this.settings = settings;
        this.enemyLayers = enemyLayers;
        this.source = source;
        direction = Vector2.up;
        isActive = true;
        hitEnemies.Clear();

        foreach (Collider2D cardCollider in GetComponentsInChildren<Collider2D>(true)) cardCollider.isTrigger = true;

        body.position = transform.position;
        body.rotation = transform.eulerAngles.z;
        body.linearVelocity = 30f * speed * direction;

        poisonTrailEmitter.Initialize(
            poisonAreaSpawner,
            settings,
            enemyLayers,
            source,
            finalDamage
        );

        Destroy(gameObject, settings.ProjectileLifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        if (TryDestroyEnemyProjectile(other)) return;
        if (!EnemyCollisionResolver.TryResolve(other, enemyLayers, out EnemyHealth enemyHealth)) return;
        if (!hitEnemies.Add(enemyHealth)) return;

        Vector2 hitPoint = other.ClosestPoint(transform.position);
        DamageInfo damageInfo = new(damage, source, hitPoint, direction);
        enemyHealth.TryTakeDamage(damageInfo);

        if (!enemyHealth.IsDead) CardEffectApplicator.Apply(enemyHealth, effect, settings, source, direction, damage);

        if (effect.DestroyOnEnemyHit) DestroyProjectile();
    }

    private bool TryDestroyEnemyProjectile(Collider2D other)
    {
        if (!effect.DestroysEnemyProjectiles) return false;

        EnemyProjectile enemyProjectile = other.GetComponentInParent<EnemyProjectile>();
        return enemyProjectile != null && enemyProjectile.TryIntercept();
    }

    private void DestroyProjectile()
    {
        if (!isActive) return;

        isActive = false;
        body.simulated = false;
        Destroy(gameObject);
    }

    private void Reset()
    {
        body = GetComponent<Rigidbody2D>();
        poisonTrailEmitter = GetComponent<PoisonCardTrailEmitter>();
    }
}
