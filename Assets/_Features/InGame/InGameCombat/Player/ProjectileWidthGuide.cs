using Sirenix.OdinInspector;
using System;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ProjectileWidthGuide : MonoBehaviour
{
    [Title("Projectile")]
    [SerializeField, MinValue(0f)] private float projectileScale = 0.3f;

    [Title("Guide")]
    [SerializeField, MinValue(0f)] private float guideLength = 50f;
    [SerializeField, MinValue(0f)] private float lineWidth = 0.1f;
    [SerializeField, MinValue(0f)] private float startDistance;
    [SerializeField, Range(0f, 0.99f)] private float fadeStart = 0.7f;
    [SerializeField, Range(0f, 1f)] private float opacity = 0.8f;
    [SerializeField] private int sortingOrder = 100;

    private readonly Vector3[] corners = new Vector3[4];

    private CardField cardField;
    private Card[] cards = Array.Empty<Card>();
    private LineRenderer leftLine;
    private LineRenderer rightLine;
    private Material lineMaterial;
    private bool isSubscribed;

    private void Awake()
    {
        lineMaterial = CreateLineMaterial();
        leftLine = CreateLine("Left Projectile Width Guide");
        rightLine = CreateLine("Right Projectile Width Guide");
        SetVisible(false);
    }

    private void Start()
    {
        cardField = CardField.Instance;
        Subscribe();
        RefreshCards();
    }

    private void OnEnable()
    {
        if (cardField == null) return;

        Subscribe();
        RefreshCards();
    }

    private void OnDisable()
    {
        Unsubscribe();
        SetVisible(false);
    }

    private void OnDestroy()
    {
        Unsubscribe();
        Destroy(lineMaterial);
    }

    private void LateUpdate()
    {
        if (cards.Length == 0) return;

        float projectileWidth = CalculateProjectileWidth();
        bool isVisible = projectileWidth > Mathf.Epsilon
            && guideLength > Mathf.Epsilon
            && lineWidth > Mathf.Epsilon
            && opacity > Mathf.Epsilon;

        SetVisible(isVisible);
        if (!isVisible) return;

        Vector3 origin = transform.position + Vector3.up * startDistance;
        Vector3 halfWidth = Vector3.right * (projectileWidth * 0.5f);
        Vector3 length = Vector3.up * guideLength;

        SetLinePositions(leftLine, origin - halfWidth, origin - halfWidth + length);
        SetLinePositions(rightLine, origin + halfWidth, origin + halfWidth + length);
    }

    private void OnValidate()
    {
        if (leftLine == null || rightLine == null) return;

        ConfigureLine(leftLine);
        ConfigureLine(rightLine);
    }

    private void Subscribe()
    {
        if (isSubscribed) return;

        cardField.CardsChanged += RefreshCards;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed) return;

        cardField.CardsChanged -= RefreshCards;
        isSubscribed = false;
    }

    private void RefreshCards()
    {
        cards = cardField.TotalCards.ToArray();
        SetVisible(cards.Length > 0);
    }

    private float CalculateProjectileWidth()
    {
        Transform layoutTransform = cards[0].transform.parent;
        float minX = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;

        foreach (Card card in cards)
        {
            RectTransform rectTransform = (RectTransform)card.transform;
            rectTransform.GetWorldCorners(corners);

            foreach (Vector3 corner in corners)
            {
                Vector3 projectileCorner = layoutTransform.rotation
                    * (layoutTransform.InverseTransformPoint(corner) * projectileScale);
                float x = projectileCorner.x;
                minX = Mathf.Min(minX, x);
                maxX = Mathf.Max(maxX, x);
            }
        }

        return maxX - minX;
    }

    private LineRenderer CreateLine(string objectName)
    {
        GameObject lineObject = new(objectName);
        lineObject.transform.SetParent(transform, false);

        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        ConfigureLine(line);
        return line;
    }

    private void ConfigureLine(LineRenderer line)
    {
        line.sharedMaterial = lineMaterial;
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.colorGradient = CreateGradient();
        line.textureMode = LineTextureMode.Stretch;
        line.alignment = LineAlignment.View;
        line.loop = false;
        line.numCapVertices = 0;
        line.sortingOrder = sortingOrder;
    }

    private Gradient CreateGradient()
    {
        Gradient gradient = new();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new[]
            {
                new GradientAlphaKey(opacity, 0f),
                new GradientAlphaKey(opacity, fadeStart),
                new GradientAlphaKey(0f, 1f)
            });
        return gradient;
    }

    private static Material CreateLineMaterial()
    {
        Material material = new(Shader.Find("Sprites/Default"))
        {
            name = "Projectile Width Guide Material",
            hideFlags = HideFlags.HideAndDontSave
        };
        return material;
    }

    private static void SetLinePositions(LineRenderer line, Vector3 start, Vector3 end)
    {
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    private void SetVisible(bool isVisible)
    {
        leftLine.enabled = isVisible;
        rightLine.enabled = isVisible;
    }
}
