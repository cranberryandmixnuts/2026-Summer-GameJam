using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CardField : SingletonBehaviour<CardField, SceneScope> {

	//======================================================================| Fields

	[SerializeField]
    private GameObject _slotPrefab;
	
	[SerializeField]
	private Transform _slotField;

	[SerializeField]
	private Transform _specialSlotField;

	[SerializeField]
	private Transform _cardField;

	[SerializeField]
	private Transform _elements;

	[SerializeField]
	private Transform _fieldBound;

    [SerializeField]
    private Vector2 _gridSize;

	[Header("Animation")]
	[SerializeField] private float _cardPlacingDuration;
	[SerializeField] private Ease _cardPlacingEase;

	[Space]
	[SerializeField] private float _cameraEffectDuration;
	[SerializeField] private Ease _cameraEffectEase;

    private readonly Dictionary<Vector2Int, Card> _placedCards = new();
    private readonly HashSet<Vector2Int> _activeSlots = new();
    private readonly Dictionary<Vector2Int, GameObject> _slotInstances = new();

	public readonly HashSet<GameObject> _specialSlots = new();

	//======================================================================| Event

	public event Action CardsChanged;
	public event Action<CardThrowArgs> OnCardThrow;

	public record CardThrowArgs(
		in float FinalDamage,
		in float Speed,
		in GameObject Cards,
		in CardEffect Effect
	);

	//======================================================================| Properties

	public float FinalBaseDamage { get; private set; } = 0f;
	public float FinalMultiplier { get; private set; } = 0f;

	public Transform SlotField => _slotField;
	public Transform SpecialSlotField => _specialSlotField;
	public Transform CardFieldTransform => _cardField;

    public IEnumerable<Vector2Int> ActiveSlots => _activeSlots;
    public IReadOnlyDictionary<Vector2Int, Card> PlacedCards => _placedCards;
    public IReadOnlyDictionary<Vector2Int, GameObject> SlotInstances => _slotInstances;

	//======================================================================| Unity Methods

	private void Start() {
        CalculateSlotPositions();
        RedrawSlots();
        CardsChanged?.Invoke();
    }

	private void OnRectTransformDimensionsChange() {
		RescaleAndMove();
	}

	private void OnEnable() {
        CardBuildingManager.OnRestarted += OnReset;
    }

    private void OnDisable() {
        CardBuildingManager.OnRestarted -= OnReset;
    }

	//======================================================================| Methods

	public bool PlaceCard(Vector2Int position, Card card) {

		if (!_activeSlots.Contains(position)) return false;

		FinalBaseDamage += card.BaseStatus.BaseDamage;
		FinalMultiplier += card.BaseStatus.AdditionalMultiplier;

		var releasedWorldPosition = card.transform.position;
		var targetLocalPosition = (Vector3)(_gridSize * position);

		card.transform.DOKill();

		_placedCards[position] = card;

		card.transform.SetParent(CardFieldTransform, true);

		CalculateSlotPositions();
		RedrawSlots();

		RescaleAndMove();

		card.transform.position = releasedWorldPosition;

		card.transform
			.DOLocalMove(targetLocalPosition, _cardPlacingDuration)
			.SetEase(_cardPlacingEase);

		card.transform
			.DOScale(Vector3.one, _cardPlacingDuration)
			.SetEase(_cardPlacingEase);

		card.transform
			.DORotate(Vector3.zero, _cardPlacingDuration)
			.SetEase(_cardPlacingEase);

		CardsChanged?.Invoke();

		return true;

	}

    private void OnReset()  {

        _placedCards.Clear();

        CalculateSlotPositions();
        RedrawSlots();
        CardsChanged?.Invoke();

		RescaleAndMove();

    }

    private void CalculateSlotPositions() {

        _activeSlots.Clear();

        if (_placedCards.Count == 0) {
            _activeSlots.Add(Vector2Int.zero);
            return;
        }

        foreach (KeyValuePair<Vector2Int, Card> pair in _placedCards) {

            if (pair.Value.BaseStatus.IsBlockingAttachment) continue;

            AddSlotIfAvailable(pair.Key + Vector2Int.up);
            AddSlotIfAvailable(pair.Key + Vector2Int.down);
            AddSlotIfAvailable(pair.Key + Vector2Int.left);
            AddSlotIfAvailable(pair.Key + Vector2Int.right);

        }

    }

    private void AddSlotIfAvailable(Vector2Int position) {

        if (_placedCards.ContainsKey(position)) return;
        _activeSlots.Add(position);

    }

    private void RedrawSlots() {

        foreach (GameObject instance in _slotInstances.Values) {
			instance.SetActive(false);
			Destroy(instance);
		}

        _slotInstances.Clear();

        foreach (Vector2Int position in _activeSlots) {

            GameObject instance = Instantiate(_slotPrefab, _slotField, false);
            instance.transform.localPosition = _gridSize * position;

            _slotInstances[position] = instance;

        }

    }

	private void RescaleAndMove() {

		var elementsRectTransform = _elements as RectTransform;
		var fieldRectTransform = _fieldBound as RectTransform;

		if (elementsRectTransform == null ||
			fieldRectTransform == null) {
			return;
		}

		elementsRectTransform.DOKill();

		var currentScale = elementsRectTransform.localScale;
		var currentPosition = elementsRectTransform.anchoredPosition;

		elementsRectTransform.localScale = Vector3.one;
		elementsRectTransform.anchoredPosition = Vector2.zero;

		var contentBound = GetContentBounds();
		var fieldBound = fieldRectTransform.GetWorldBounds();

		if (contentBound.size.x <= Mathf.Epsilon ||
			contentBound.size.y <= Mathf.Epsilon) {

			elementsRectTransform.localScale = currentScale;
			elementsRectTransform.anchoredPosition = currentPosition;
			return;

		}

		var targetScale = Mathf.Min(
			1f,
			fieldBound.size.x / contentBound.size.x,
			fieldBound.size.y / contentBound.size.y
		);

		elementsRectTransform.localScale = Vector3.one * targetScale;

		contentBound = GetContentBounds();

		var worldOffset = fieldBound.center - contentBound.center;
		var localOffset = elementsRectTransform.parent.InverseTransformVector(worldOffset);
		var targetPosition = (Vector2)localOffset;

		elementsRectTransform.localScale = currentScale;
		elementsRectTransform.anchoredPosition = currentPosition;

		elementsRectTransform
			.DOScale(Vector3.one * targetScale, _cameraEffectDuration)
			.SetEase(_cameraEffectEase);

		elementsRectTransform
			.DOAnchorPos(targetPosition, _cameraEffectDuration)
			.SetEase(_cameraEffectEase);

	}

	private Bounds GetContentBounds() {

		var cardStates = new List<(
			Transform Transform,
			Vector3 LocalPosition,
			Vector3 LocalScale
		)>(_placedCards.Count);

		foreach (var pair in _placedCards) {

			var cardTransform = pair.Value.transform;

			cardStates.Add((
				cardTransform,
				cardTransform.localPosition,
				cardTransform.localScale
			));

			cardTransform.localPosition = _gridSize * pair.Key;
			cardTransform.localScale = Vector3.one;

		}

		try {

			var cardsBound = (_cardField as RectTransform).GetGlobalBounds();
			var slotBound = (_slotField as RectTransform).GetGlobalBounds();
			var specialSlotBound = (_specialSlotField as RectTransform).GetGlobalBounds();

			var bound = cardsBound;

			bound.Encapsulate(slotBound.min);
			bound.Encapsulate(slotBound.max);

			bound.Encapsulate(specialSlotBound.min);
			bound.Encapsulate(specialSlotBound.max);

			return bound;

		}
		finally {

			foreach (var state in cardStates) {

				state.Transform.localPosition = state.LocalPosition;
				state.Transform.localScale = state.LocalScale;

			}

		}

	}

	private void OnDrawGizmos() {

		var contentBound = GetContentBounds();
		Gizmos.color = Color.skyBlue;
		Gizmos.DrawWireCube(contentBound.center, contentBound.size);

	}

}
