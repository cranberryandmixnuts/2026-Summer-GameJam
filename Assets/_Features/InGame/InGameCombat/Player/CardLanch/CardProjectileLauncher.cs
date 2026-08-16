using Sirenix.OdinInspector;
using UnityEngine;
using static CardField;

public sealed class CardProjectileLauncher : MonoBehaviour
{
    [SerializeField, Required, AssetsOnly] private CardProjectile projectilePrefab;
    [SerializeField, Required, InlineEditor] private CardProjectileSettings settings;
    [SerializeField, Required] private PoisonAreaPool poisonAreaPool;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField, Required] private Transform target;
    [SerializeField, MinValue(0f)] private float projectileScale = 0.3f;

    private void OnEnable()
    {
        CardField.Instance.OnCardThrow += HandleCardThrow;
    }

    private void OnDisable()
    {
        if (CardField.Instance != null) CardField.Instance.OnCardThrow -= HandleCardThrow;
    }

    private void HandleCardThrow(CardThrowArgs args)
    {
        CardProjectile projectile = Instantiate(projectilePrefab);
        Transform projectileTransform = projectile.transform;

        CardProjectileGroupBuilder.AttachCards(args.Cards, projectileTransform);

        projectileTransform.SetParent(target, true);
        projectileTransform.localScale =
            Vector3.one * projectileScale * args.Effect.SizeMultiplier;
        projectileTransform.position = transform.position;

        int finalDamage = Mathf.Max(0, Mathf.RoundToInt(args.FinalDamage));
        float speed = Mathf.Max(0f, args.Speed * args.Effect.SpeedMutliplier);

        projectile.Initialize(
            finalDamage,
            speed,
            args.Effect,
            settings,
            poisonAreaPool,
            enemyLayers,
            gameObject
        );
    }

    private void Reset()
    {
        poisonAreaPool = GetComponent<PoisonAreaPool>();
        enemyLayers = LayerMask.GetMask("Enemy");
    }
}
