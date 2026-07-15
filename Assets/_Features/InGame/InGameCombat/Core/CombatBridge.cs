using System;
using UnityEngine;

public sealed class CombatBridge : SingletonBehaviour<CombatBridge, SceneScope>
{
    [SerializeField, Min(EnemyDifficultyUtility.MinimumDifficultyFactor), InspectorName("외부 난이도 인수")]
    private float externalDifficultyFactor = 1f;

    public event Action<float> PlayerDamaged;
    public event Action PlayerDied;
    public event Action FireRequested;

    public float ExternalDifficultyFactor
    {
        get => externalDifficultyFactor;
        set => externalDifficultyFactor = EnemyDifficultyUtility.ClampFactor(value);
    }

    public void PublishPlayerDamaged(float rate)
    {
        Debug.Log("아파");
        PlayerDamaged?.Invoke(rate);
    }

    public void PublishPlayerDied()
    {
        Debug.Log("사망");
        PlayerDied?.Invoke();
    }

    public void RequestFire()
    {
        Debug.Log("발싸!!!!!!");
        FireRequested?.Invoke();
    }

    private void OnValidate() =>
        externalDifficultyFactor = EnemyDifficultyUtility.ClampFactor(externalDifficultyFactor);
}
