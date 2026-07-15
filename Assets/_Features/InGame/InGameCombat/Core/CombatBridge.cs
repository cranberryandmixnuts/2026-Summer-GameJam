using System;
using UnityEngine;

public sealed class CombatBridge : SingletonBehaviour<CombatBridge, SceneScope>
{
    public event Action<float> PlayerDamaged;
    public event Action PlayerDied;
    public event Action FireRequested;

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
}
