using UnityEngine;
using UnityEngine.InputSystem;

public sealed class MouseInverseParallax : MonoBehaviour
{
    [SerializeField] private float maxMoveDistance = 20f;
    [SerializeField] private float smoothSpeed = 8f;

    private RectTransform rectTransform;
    private RectTransform parentRectTransform;
    private Canvas canvas;
    private Vector2 initialPosition;

    private void Awake()
    {
        rectTransform = (RectTransform)transform;
        parentRectTransform = (RectTransform)rectTransform.parent;
        canvas = GetComponentInParent<Canvas>();
        initialPosition = rectTransform.anchoredPosition;
    }

    private void LateUpdate()
    {
        Vector2 mousePosition = Pointer.current.position.ReadValue();
        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRectTransform,
            mousePosition,
            uiCamera,
            out Vector2 localMousePosition
        );

        Vector2 center = parentRectTransform.rect.center;
        Vector2 halfSize = parentRectTransform.rect.size * 0.5f;
        Vector2 mouseOffset = localMousePosition - center;

        Vector2 normalizedOffset = new(
            Mathf.Clamp(mouseOffset.x / halfSize.x, -1f, 1f),
            Mathf.Clamp(mouseOffset.y / halfSize.y, -1f, 1f)
        );

        Vector2 targetPosition = initialPosition - normalizedOffset * maxMoveDistance;

        rectTransform.anchoredPosition = Vector2.Lerp(
            rectTransform.anchoredPosition,
            targetPosition,
            1f - Mathf.Exp(-smoothSpeed * Time.unscaledDeltaTime)
        );
    }
}