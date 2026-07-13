using Sirenix.OdinInspector;
using UnityEngine;

[EnemyBehaviorMenu("플레이어/감지 영역 겹침")]
[System.Serializable]
public sealed class PlayerColliderOverlapEnemyCondition : EnemyCondition
{
    [SerializeField, Required, EnemyBehaviorField("감지 영역")] private Collider2D detectionArea;

    public override bool Evaluate(in EnemyBehaviorContext context) =>
        detectionArea.Distance(context.PlayerCollider).isOverlapped;
}
