using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class CardProjectile : MonoBehaviour
{
    private readonly HashSet<EnemyHealth> hitEnemies = new();

    private CardEffect effect;
    private CardProjectileSettings settings;
    private PoisonAreaPool poisonAreaPool;
    private LayerMask enemyLayers;
    private GameObject source;
    private int damage;
    private float poisonDropElapsedTime;

    public void Initialize(
        Rigidbody2D body,
        int damage,
        float speed,
        CardEffect effect,
        CardProjectileSettings settings,
        PoisonAreaPool poisonAreaPool,
        LayerMask enemyLayers,
        GameObject source
    )
    {
        this.damage = damage;
        this.effect = effect;
        this.settings = settings;
        this.poisonAreaPool = poisonAreaPool;
        this.enemyLayers = enemyLayers;
        this.source = source;
        poisonDropElapsedTime = 0f;
        hitEnemies.Clear();

        foreach (Collider2D cardCollider in GetComponentsInChildren<Collider2D>(true)) cardCollider.isTrigger = true;

        body.linearVelocity = Vector2.up * speed;
        Destroy(gameObject, settings.ProjectileLifetime);
    }

    private void Update()
    {
        if (!effect.IsPoison) return;

        poisonDropElapsedTime += Time.deltaTime;

        while (poisonDropElapsedTime >= settings.PoisonDropInterval)
        {
            poisonDropElapsedTime -= settings.PoisonDropInterval;
            poisonAreaPool.Spawn(transform.position, settings, enemyLayers, source);
        }
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
