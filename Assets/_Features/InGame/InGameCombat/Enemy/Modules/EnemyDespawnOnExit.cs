using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[MovedFrom(false, sourceClassName: "DestroyBelowWorldY")]
public sealed class EnemyDespawnOnExit : BaseBehaviour,
    IEnemyRuntimeInitializable,
    IEnemyDifficultyInitializable
{
    [SerializeField, Required] private Collider2D bodyCollider;
    [SerializeField, MinValue(1)] private int escapeDamage = 1;

    private EnemyRuntimeContext runtimeContext;
    private float difficultyFactor = 1f;
    private bool isInitialized;
    private bool hasExited;

    public void Initialize(in EnemyRuntimeContext context)
    {
        runtimeContext = context;
        isInitialized = true;
    }

    public void InitializeDifficulty(float value) =>
        difficultyFactor = EnemyDifficultyUtility.ClampFactor(value);

    private void FixedUpdate()
    {
        if (!isInitialized || hasExited) return;
        if (runtimeContext.DespawnBounds.Overlaps(bodyCollider)) return;

        hasExited = true;

        Vector2 hitPoint = runtimeContext.PlayerCollider.ClosestPoint(transform.position);
        Vector2 direction = ((Vector2)runtimeContext.Player.position - (Vector2)transform.position).normalized;
        int finalEscapeDamage = EnemyDifficultyUtility.ScaleStat(
            escapeDamage,
            difficultyFactor);
        DamageInfo damageInfo = new(finalEscapeDamage, gameObject, hitPoint, direction);

        runtimeContext.PlayerHealth.TryTakeDamage(damageInfo);
        Destroy(gameObject);
    }
}
