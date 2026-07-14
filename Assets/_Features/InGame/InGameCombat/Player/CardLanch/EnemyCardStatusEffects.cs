using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(EnemyBrain))]
public sealed class EnemyCardStatusEffects : MonoBehaviour
{
    private const float FireTickInterval = 1f;

    private EnemyHealth health;
    private EnemyBrain brain;

    private int fireLevel;
    private int fireDamagePerSecond;
    private float fireRemainingDuration;
    private float fireTickRemaining;
    private GameObject fireSource;

    private int waterLevel;
    private float waterRemainingDuration;

    private int electricLevel;
    private float electricRemainingDuration;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        brain = GetComponent<EnemyBrain>();
    }

    public void Apply(CardEffect effect, CardProjectileSettings settings, GameObject source)
    {
        if (effect.FireLevel > 0) ApplyFire(effect.FireLevel, settings, source);

        if (effect.WaterLevel > 0) ApplyWater(effect.WaterLevel, settings);

        if (effect.ElectricLevel > 0) ApplyElectric(effect.ElectricLevel, settings);
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        UpdateFire(deltaTime);
        UpdateWater(deltaTime);
        UpdateElectric(deltaTime);
    }

    private void ApplyFire(int addedLevel, CardProjectileSettings settings, GameObject source)
    {
        bool isActive = fireRemainingDuration > 0f;
        fireLevel = isActive ? fireLevel + addedLevel : addedLevel;
        fireDamagePerSecond = settings.FireDamagePerLevel * fireLevel;
        fireRemainingDuration = settings.FireDurationPerLevel * fireLevel;
        fireSource = source;

        if (!isActive) fireTickRemaining = FireTickInterval;
    }

    private void ApplyWater(int addedLevel, CardProjectileSettings settings)
    {
        bool isActive = waterRemainingDuration > 0f;
        waterLevel = isActive ? waterLevel + addedLevel : addedLevel;
        waterRemainingDuration = settings.WaterDurationPerLevel * waterLevel;
        brain.TransitionDelay = settings.WaterTransitionDelayPerLevel * waterLevel;
    }

    private void ApplyElectric(int addedLevel, CardProjectileSettings settings)
    {
        bool isActive = electricRemainingDuration > 0f;
        electricLevel = isActive ? electricLevel + addedLevel : addedLevel;
        electricRemainingDuration = settings.ElectricDurationPerLevel * electricLevel;

        float slowRatio = settings.ElectricSlowPercentPerLevel * 0.01f * electricLevel;
        brain.MovementSpeedMultiplier = Mathf.Max(0f, 1f - slowRatio);
    }

    private void UpdateFire(float deltaTime)
    {
        if (fireLevel <= 0) return;

        float activeDeltaTime = Mathf.Min(deltaTime, fireRemainingDuration);
        fireRemainingDuration -= deltaTime;
        fireTickRemaining -= activeDeltaTime;

        while (fireTickRemaining <= 0f && !health.IsDead)
        {
            DamageInfo damageInfo = new(fireDamagePerSecond, fireSource, transform.position, Vector2.zero);
            health.TryTakeDamage(damageInfo);
            fireTickRemaining += FireTickInterval;
        }

        if (fireRemainingDuration > 0f && !health.IsDead) return;

        fireLevel = 0;
        fireDamagePerSecond = 0;
        fireRemainingDuration = 0f;
        fireTickRemaining = 0f;
        fireSource = null;
    }

    private void UpdateWater(float deltaTime)
    {
        if (waterLevel <= 0) return;

        waterRemainingDuration -= deltaTime;
        if (waterRemainingDuration > 0f) return;

        ResetWater();
    }

    private void UpdateElectric(float deltaTime)
    {
        if (electricLevel <= 0) return;

        electricRemainingDuration -= deltaTime;
        if (electricRemainingDuration > 0f) return;

        ResetElectric();
    }

    private void ResetWater()
    {
        if (waterLevel <= 0) return;

        waterLevel = 0;
        waterRemainingDuration = 0f;
        brain.TransitionDelay = 0f;
    }

    private void ResetElectric()
    {
        if (electricLevel <= 0) return;

        electricLevel = 0;
        electricRemainingDuration = 0f;
        brain.MovementSpeedMultiplier = 1f;
    }

    private void OnDisable()
    {
        fireLevel = 0;
        fireDamagePerSecond = 0;
        fireRemainingDuration = 0f;
        fireTickRemaining = 0f;
        fireSource = null;
        ResetWater();
        ResetElectric();
    }
}
