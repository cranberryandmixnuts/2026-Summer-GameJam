using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "CardProjectileSettings", menuName = "Game/Card/Card Projectile Settings")]
public sealed class CardProjectileSettings : ScriptableObject
{
    [Header("Projectile")]
    [SerializeField, MinValue(0.1f)] private float projectileLifetime = 10f;

    [Header("Fire")]
    [SerializeField, MinValue(0)] private int fireDamagePerLevel = 1;
    [SerializeField, MinValue(0f)] private float fireDurationPerLevel = 3f;

    [Header("Water")]
    [SerializeField, MinValue(0f)] private float waterTransitionDelayPerLevel = 0.1f;
    [SerializeField, MinValue(0f)] private float waterDurationPerLevel = 3f;

    [Header("Electric")]
    [SerializeField, Range(0f, 100f)] private float electricSlowPercentPerLevel = 10f;
    [SerializeField, MinValue(0f)] private float electricDurationPerLevel = 3f;

    [Header("Poison")]
    [SerializeField, MinValue(0.01f)] private float poisonDropInterval = 0.5f;
    [SerializeField, MinValue(0)] private int poisonAreaDamage = 1;
    [SerializeField, MinValue(0.01f)] private float poisonDamageInterval = 1f;
    [SerializeField, MinValue(0f)] private float poisonAreaDuration = 3f;

    [Header("Heal")]
    [SerializeField, Range(0f, 100f)] private float healPercentPerLevel = 5f;

    public float ProjectileLifetime => projectileLifetime;
    public int FireDamagePerLevel => fireDamagePerLevel;
    public float FireDurationPerLevel => fireDurationPerLevel;
    public float WaterTransitionDelayPerLevel => waterTransitionDelayPerLevel;
    public float WaterDurationPerLevel => waterDurationPerLevel;
    public float ElectricSlowPercentPerLevel => electricSlowPercentPerLevel;
    public float ElectricDurationPerLevel => electricDurationPerLevel;
    public float PoisonDropInterval => poisonDropInterval;
    public int PoisonAreaDamage => poisonAreaDamage;
    public float PoisonDamageInterval => poisonDamageInterval;
    public float PoisonAreaDuration => poisonAreaDuration;
    public float HealPercentPerLevel => healPercentPerLevel;
}
