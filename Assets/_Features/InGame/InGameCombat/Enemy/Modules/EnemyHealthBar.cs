using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public sealed class EnemyHealthBar : MonoBehaviour
{
    [SerializeField, Required] private Image fillImage;

    private RectTransform rectTransform;
    private RectTransform barsRoot;
    private Canvas canvas;
    private Camera worldCamera;
    private EnemyHealth health;
    private Transform target;
    private Vector3 offset;

    public void Initialize(
        EnemyHealth sourceHealth,
        RectTransform sourceBarsRoot,
        Camera sourceWorldCamera,
        Vector3 sourceOffset)
    {
        rectTransform = (RectTransform)transform;
        barsRoot = sourceBarsRoot;
        canvas = barsRoot.GetComponentInParent<Canvas>();
        worldCamera = sourceWorldCamera;
        health = sourceHealth;
        target = health.transform;
        offset = sourceOffset;

        health.Damaged += HandleDamaged;
        health.Healed += HandleHealed;
        health.Died += HandleDied;

        Refresh();
        UpdatePosition();
    }

    private void LateUpdate()
    {
        if (!target)
        {
            Destroy(gameObject);
            return;
        }

        UpdatePosition();
    }

    private void OnDestroy() => Unsubscribe();

    private void HandleDamaged(DamageInfo damageInfo) => Refresh();

    private void HandleHealed(int amount) => Refresh();

    private void HandleDied(EnemyHealth deadHealth)
    {
        Unsubscribe();
        Destroy(gameObject);
    }

    private void Refresh() => fillImage.fillAmount = (float)health.CurrentHealth / health.MaxHealth;

    private void UpdatePosition()
    {
        Vector3 worldPosition = target.position + offset;

        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            rectTransform.position = worldPosition;
            return;
        }

        Camera canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(worldCamera, worldPosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            barsRoot,
            screenPosition,
            canvasCamera,
            out Vector2 localPosition);
        rectTransform.anchoredPosition = localPosition;
    }

    private void Unsubscribe()
    {
        if (!health) return;

        health.Damaged -= HandleDamaged;
        health.Healed -= HandleHealed;
        health.Died -= HandleDied;
        health = null;
        target = null;
    }
}