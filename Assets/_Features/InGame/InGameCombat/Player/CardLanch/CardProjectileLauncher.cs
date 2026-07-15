using Sirenix.OdinInspector;
using UnityEngine;
using static CardField;

[DisallowMultipleComponent]
public sealed class CardProjectileLauncher : MonoBehaviour
{
    [SerializeField, Required, InlineEditor] private CardProjectileSettings settings;
    [SerializeField, Required] private PoisonAreaPool poisonAreaPool;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private Transform Canvas;

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
        Debug.Log($"CardProjectileLauncher.HandleCardThrow: {args.FinalDamage}, {args.Speed}, {args.Effect}");
        GameObject cards = args.Cards;
        Transform cardsTransform = cards.transform;
        cardsTransform.SetParent(Canvas, true);
        cardsTransform.position = transform.position;

        Rigidbody2D body = GetOrAddBody(cards);
        ConfigureBody(body);
        body.position = transform.position;

        CardProjectile projectile = GetOrAddProjectile(cards);
        int damage = Mathf.Max(0, Mathf.RoundToInt(args.FinalDamage));
        float speed = Mathf.Max(0f, args.Speed);

        projectile.Initialize(
            body,
            damage,
            speed,
            args.Effect,
            settings,
            poisonAreaPool,
            enemyLayers,
            gameObject
        );
    }

    private static Rigidbody2D GetOrAddBody(GameObject cards)
    {
        if (cards.TryGetComponent(out Rigidbody2D body)) return body;

        return cards.AddComponent<Rigidbody2D>();
    }

    private static CardProjectile GetOrAddProjectile(GameObject cards)
    {
        if (cards.TryGetComponent(out CardProjectile projectile)) return projectile;

        return cards.AddComponent<CardProjectile>();
    }

    private static void ConfigureBody(Rigidbody2D body)
    {
        body.bodyType = RigidbodyType2D.Dynamic;
        body.simulated = true;
        body.gravityScale = 0f;
        body.linearDamping = 0f;
        body.angularDamping = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
    }

    private void Reset()
    {
        poisonAreaPool = GetComponent<PoisonAreaPool>();
        enemyLayers = LayerMask.GetMask("Enemy");
    }
}
