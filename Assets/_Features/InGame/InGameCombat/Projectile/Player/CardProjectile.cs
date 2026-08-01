using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class CardProjectile : MonoBehaviour
{
    private readonly HashSet<EnemyHealth> hitEnemies = new();

    private CardEffect effect;
    private CardProjectileSettings settings;
    private LayerMask enemyLayers;
    private GameObject source;
    private int damage;

    public void Initialize(
        Rigidbody2D body,
        int damage,
        float speed,
        CardEffect effect,
        CardProjectileSettings settings,
        LayerMask enemyLayers,
        GameObject source
    )
    {
        this.damage = damage;
        this.effect = effect;
        this.settings = settings;
        this.enemyLayers = enemyLayers;
        this.source = source;
        hitEnemies.Clear();

        foreach (Collider2D cardCollider in GetComponentsInChildren<Collider2D>(true)) cardCollider.isTrigger = true;

        body.linearVelocity = 30 * speed * Vector2.up;
        Destroy(gameObject, settings.ProjectileLifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!EnemyCollisionResolver.TryResolve(other, enemyLayers, out EnemyHealth enemyHealth)) return;
        if (!hitEnemies.Add(enemyHealth)) return;

        Vector2 hitPoint = other.ClosestPoint(transform.position);
        DamageInfo damageInfo = new(damage, source, hitPoint, Vector2.up);
        enemyHealth.TryTakeDamage(damageInfo);

        if (enemyHealth.IsDead) return;

        CardEffectApplicator.Apply(enemyHealth, effect, settings, source);
    }
}
