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
        public PoisonAreaDamageReceiver DamageReceiver { get; }
        public int OverlapCount { get; set; }

        public Target(EnemyHealth health, PoisonAreaDamageReceiver damageReceiver)
        {
            Health = health;
            DamageReceiver = damageReceiver;
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
        int finalProjectileDamage
    )
    {
        this.settings = settings;
        this.enemyLayers = enemyLayers;
        this.source = source;
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
            .OnComplete(DestroyArea);
        }
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

        PoisonAreaDamageReceiver damageReceiver = enemyHealth.GetComponent<PoisonAreaDamageReceiver>();

        damageReceiver.Register(
            this,
            tickDamage,
            settings.PoisonDamageInterval,
            source);

        if (!enemyHealth.IsDead) targets.Add(new Target(enemyHealth, damageReceiver));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!EnemyCollisionResolver.TryResolve(other, enemyLayers, out EnemyHealth enemyHealth)) return;

        Target target = FindTarget(enemyHealth);
        if (target == null) return;

        target.OverlapCount--;

        if (target.OverlapCount > 0) return;

        if (target.DamageReceiver != null) target.DamageReceiver.Unregister(this);
        targets.Remove(target);
    }

    private Target FindTarget(EnemyHealth enemyHealth)
    {
        foreach (Target target in targets)
        {
            if (target.Health == enemyHealth) return target;
        }

        return null;
    }

    private void DestroyArea()
    {
        UnregisterTargets();
        isActive = false;
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private void UnregisterTargets()
    {
        foreach (Target target in targets)
        {
            if (target.DamageReceiver != null) target.DamageReceiver.Unregister(this);
        }

        targets.Clear();
    }

    private void OnDisable()
    {
        isActive = false;
        tween?.Kill();
        tween = null;
        UnregisterTargets();
        settings = null;
        source = null;
        enemyLayers = default;
        tickDamage = 0;
    }

    private void OnDestroy()
    {
        if (material != null) Destroy(material);
    }

    private void Reset() => areaCollider = GetComponent<Collider2D>();
}
