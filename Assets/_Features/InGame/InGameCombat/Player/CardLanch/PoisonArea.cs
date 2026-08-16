using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

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
    [SerializeField] private float _startingAnimationDuration;
    [SerializeField] private Ease _startingAnimationEase;
    [SerializeField] private float _removeingAnimationDuration;
    [SerializeField] private Ease _removeingAnimationEase;

    private readonly List<Target> targets = new();

    private Tween tween;
    private Material material;
    private CardProjectileSettings settings;
    private LayerMask enemyLayers;
    private GameObject source;
    private Action<PoisonArea> releaseAction;
    private int tickDamage;
    private float remainingLifetime;
    private bool isActive;

    private void Awake()
    {
        areaCollider.isTrigger = true;
        material = GetComponent<SpriteRenderer>().material;
    }

    public void Activate(
        Vector3 position,
        CardProjectileSettings settings,
        LayerMask enemyLayers,
        GameObject source,
        int finalProjectileDamage,
        Action<PoisonArea> releaseAction
    )
    {
        this.settings = settings;
        this.enemyLayers = enemyLayers;
        this.source = source;
        this.releaseAction = releaseAction;
        tickDamage = settings.CalculatePoisonTickDamage(finalProjectileDamage);
        remainingLifetime = settings.PoisonAreaDuration;
        isActive = true;
        targets.Clear();

        transform.SetPositionAndRotation(position, Quaternion.identity);
        gameObject.SetActive(true);

        tween?.Kill();
        material.SetFloat("_Display", 0f);
        tween = DOTween.To(
            () => material.GetFloat("_Display"),
            value => material.SetFloat("_Display", value),
            1f,
            _startingAnimationDuration
        ).SetEase(_startingAnimationEase);
    }

    private void Update()
    {
        if (!isActive) return;

        float deltaTime = Time.deltaTime;
        remainingLifetime -= deltaTime;

        if (remainingLifetime <= _removeingAnimationDuration)
        {
            remainingLifetime = float.MaxValue;
            tween = DOTween.To(
                () => material.GetFloat("_Display"),
                value => material.SetFloat("_Display", value),
                0f,
                _removeingAnimationDuration
            )
            .SetEase(_removeingAnimationEase)
            .OnComplete(Release);
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

            if (target.Health.IsDead) targets.Remove(target);
        }
    }

    private void DealDamage(EnemyHealth enemyHealth)
    {
        DamageInfo damageInfo = new(
            tickDamage,
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
        tween?.Kill();
    }

    private void OnDisable()
    {
        isActive = false;
        targets.Clear();
        settings = null;
        source = null;
        releaseAction = null;
        enemyLayers = default;
        tickDamage = 0;
    }

    private void Reset() => areaCollider = GetComponent<Collider2D>();
}
