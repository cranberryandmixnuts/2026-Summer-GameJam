using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(false, sourceClassName: "DestroyBelowWorldY")]
[DisallowMultipleComponent]
public sealed class EnemyDespawnOnExit : MonoBehaviour, IEnemyRuntimeInitializable
{
    [SerializeField, Required] private Collider2D bodyCollider;
    [SerializeField, MinValue(1)] private int escapeDamage = 1;

    private EnemyRuntimeContext runtimeContext;
    private bool isInitialized;
    private bool hasExited;

    public void Initialize(in EnemyRuntimeContext context)
    {
        runtimeContext = context;
        isInitialized = true;
    }

    private void FixedUpdate()
    {
        if (!isInitialized || hasExited) return;
        if (runtimeContext.DespawnBounds.Overlaps(bodyCollider)) return;

        hasExited = true;

        if (runtimeContext.IsCombatActive)
        {
            Vector2 hitPoint = runtimeContext.PlayerCollider.ClosestPoint(transform.position);
            Vector2 direction = ((Vector2)runtimeContext.Player.position - (Vector2)transform.position).normalized;
            DamageInfo damageInfo = new(escapeDamage, gameObject, hitPoint, direction);

            runtimeContext.PlayerHealth.TryTakeDamage(damageInfo);
        }

        Destroy(gameObject);
    }
}
