using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public sealed class CardHandLineVisualizer : MonoBehaviour
{
    private sealed class LineBinding
    {
        public LineRenderer Renderer { get; }
        public CardHandMatch Match { get; set; }

        public LineBinding(LineRenderer renderer)
        {
            Renderer = renderer;
        }
    }

    [SerializeField]
    [Required]
    private CardHandDetector _detector;

    [SerializeField]
    [Required]
    private Material _lineMaterial;

    [SerializeField]
    [Min(0.001f)]
    private float _lineWidth = 0.05f;

    [SerializeField]
    private Vector3 _worldOffset = new(0f, 0f, -0.1f);

    [SerializeField]
    private string _sortingLayerName = "Default";

    [SerializeField]
    private int _sortingOrder = 100;

    private readonly List<LineBinding> _bindings = new();
    private int _activeBindingCount;

    private void OnEnable()
    {
        _detector.MatchesChanged += Rebuild;
        Rebuild(_detector.CurrentMatches);
    }

    private void OnDisable()
    {
        _detector.MatchesChanged -= Rebuild;
        DisableAllBindings();
    }

    private void LateUpdate()
    {
        for (int i = 0; i < _activeBindingCount; i++) UpdatePositions(_bindings[i]);
    }

    private void OnDestroy()
    {
        foreach (LineBinding binding in _bindings) Destroy(binding.Renderer.gameObject);
    }

    private void Rebuild(IReadOnlyList<CardHandMatch> matches)
    {
        EnsureBindingCount(matches.Count);
        _activeBindingCount = matches.Count;

        for (int i = 0; i < matches.Count; i++)
        {
            LineBinding binding = _bindings[i];
            CardHandMatch match = matches[i];

            binding.Match = match;
            binding.Renderer.positionCount = match.Cards.Count;
            binding.Renderer.startColor = match.LineColor;
            binding.Renderer.endColor = match.LineColor;
            binding.Renderer.enabled = true;

            UpdatePositions(binding);
        }

        for (int i = matches.Count; i < _bindings.Count; i++) _bindings[i].Renderer.enabled = false;
    }

    private void EnsureBindingCount(int requiredCount)
    {
        while (_bindings.Count < requiredCount) _bindings.Add(CreateBinding(_bindings.Count));
    }

    private LineBinding CreateBinding(int index)
    {
        GameObject instance = new($"CardHandLine_{index}")
        {
            layer = gameObject.layer
        };
        instance.transform.SetParent(transform, false);

        LineRenderer lineRenderer = instance.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.sharedMaterial = _lineMaterial;
        lineRenderer.startWidth = _lineWidth;
        lineRenderer.endWidth = _lineWidth;
        lineRenderer.numCapVertices = 4;
        lineRenderer.numCornerVertices = 4;
        lineRenderer.alignment = LineAlignment.View;
        lineRenderer.textureMode = LineTextureMode.Stretch;
        lineRenderer.sortingLayerID = SortingLayer.NameToID(_sortingLayerName);
        lineRenderer.sortingOrder = _sortingOrder;

        return new LineBinding(lineRenderer);
    }

    private void UpdatePositions(LineBinding binding)
    {
        for (int i = 0; i < binding.Match.Cards.Count; i++)
        {
            Vector3 position = binding.Match.Cards[i].Card.transform.position + _worldOffset;
            binding.Renderer.SetPosition(i, position);
        }
    }

    private void DisableAllBindings()
    {
        _activeBindingCount = 0;

        foreach (LineBinding binding in _bindings) binding.Renderer.enabled = false;
    }
}