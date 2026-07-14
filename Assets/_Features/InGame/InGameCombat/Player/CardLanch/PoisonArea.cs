using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class PoisonArea : MonoBehaviour
{
    private sealed class Target
    {
        public EnemyHealth Health { get; }
        public float DamageTimeRemaining { get; set; }
        public int OverlapCount { get; set; }

        public Target(EnemyHealth health, float damageTimeRemaining)
        {
            Health = health;
            DamageTimeRemaining = damageTimeRemaining;
            OverlapCount = 1;
        }
    }

    [SerializeField, Required] private Collider2D areaCollider;

    private readonly List<Target> targets = new();

    private CardProjectileSettings settings;
    private LayerMask enemyLayers;
    private GameObject source;
    private Action<PoisonArea> releaseAction;
    private float remainingLifetime;
    private bool isActive;

    private void Awake() => areaCollider.isTrigger = true;

    public void Activate(
        Vector3 position,
        CardProjectileSettings settings,
        LayerMask enemyLayers,
        GameObject source,
        Action<PoisonArea> releaseAction
    )
    {
        this.settings = settings;
        this.enemyLayers = enemyLayers;
        this.source = source;
        this.releaseAction = releaseAction;
        remainingLifetime = settings.PoisonAreaDuration;
        isActive = true;
        targets.Clear();
        transform.SetPositionAndRotation(position, Quaternion.identity);
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!isActive) return;

        float deltaTime = Time.deltaTime;
        remainingLifetime -= deltaTime;

        if (remainingLifetime <= 0f)
        {
            Release();
            return;
        }

        UpdateTargets(deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!EnemyCollisionResolver.TryResolve(other, enemyLayers, out EnemyHealth enemyHealth)) return;
        if (enemyHealth.IsDead) return;

        Target existingTarget = FindTarget(enemyHealth);

        if (existingTarget != null)
        {
            existingTarget.OverlapCount++;
            return;
        }

        DealDamage(enemyHealth);

        if (!enemyHealth.IsDead) targets.Add(new Target(enemyHealth, settings.PoisonDamageInterval));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!EnemyCollisionResolver.TryResolve(other, enemyLayers, out EnemyHealth enemyHealth)) return;

        Target target = FindTarget(enemyHealth);
        if (target == null) return;

        target.OverlapCount--;
        if (target.OverlapCount <= 0) targets.Remove(target);
    }

    private void UpdateTargets(float deltaTime)
    {
        for (int index = targets.Count - 1; index >= 0; index--)
        {
            Target target = targets[index];

            if (target.Health == null || target.Health.IsDead)
            {
                targets.RemoveAt(index);
                continue;
            }

            target.DamageTimeRemaining -= deltaTime;

            while (target.DamageTimeRemaining <= 0f && !target.Health.IsDead)
            {
                DealDamage(target.Health);
                target.DamageTimeRemaining += settings.PoisonDamageInterval;
            }

            if (target.Health.IsDead) targets.RemoveAt(index);
        }
    }

    private void DealDamage(EnemyHealth enemyHealth)
    {
        DamageInfo damageInfo = new(
            settings.PoisonAreaDamage,
            source,
            enemyHealth.transform.position,
            Vector2.zero
        );

        enemyHealth.TryTakeDamage(damageInfo);
    }

    private Target FindTarget(EnemyHealth enemyHealth)
    {
        foreach (Target target in targets)
        {
            if (target.Health == enemyHealth) return target;
        }

        return null;
    }

    private void Release()
    {
        isActive = false;
        releaseAction(this);
    }

    private void OnDisable()
    {
        isActive = false;
        targets.Clear();
        settings = null;
        source = null;
        releaseAction = null;
        enemyLayers = default;
    }

    private void Reset() => areaCollider = GetComponent<Collider2D>();
}
