using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CardField : SingletonBehaviour<CardField, SceneScope> {

	//======================================================================| Fields

	[SerializeField]
    private GameObject _slotPrefab;

	[SerializeField]
	private GameObject _specialSlotPrefab;
	
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
    private readonly Dictionary<GameObject, Vector2Int> _slotInstances = new();

	private readonly Dictionary<Image, SpecialCardSlot> _specialSlotInstances = new();
	private readonly HashSet<Card> _specialPlacedCards = new();

	//======================================================================| Properties

	public float FinalBaseDamage { get; private set; } = 0f;
	public float FinalMultiplier { get; private set; } = 1f;

	public Transform SlotField => _slotField;
	public Transform SpecialSlotField => _specialSlotField;
	public Transform CardFieldTransform => _cardField;

    public IEnumerable<Vector2Int> ActiveSlots => _activeSlots;
    public IReadOnlyDictionary<Vector2Int, Card> PlacedCards => _placedCards;
    public IReadOnlyDictionary<GameObject, Vector2Int> SlotInstances => _slotInstances;
	public IEnumerable<SpecialCardSlot> SpecialSlotInstances => _specialSlotInstances.Values;

	public IEnumerable<Card> TotalCards => PlacedCards.Values.Concat(_specialPlacedCards);
	
	//======================================================================| Event

	public event Action CardsChanged;
	public event Action<CardThrowArgs> OnCardThrow;

	public record CardThrowArgs(
		in float FinalDamage,
		in float Speed,
		in GameObject Cards,
		in CardEffect Effects
	);

	//======================================================================| Unity Methods

	private void Start() {
        CalculateSlotPositions();
        RedrawSlots();
        CardsChanged?.Invoke();
    }

	private void Update() {		
		SyncSpecialSlotTransforms();
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

	public bool PlaceCard(GameObject target, Card card) {

		var releasedWorldPosition = card.transform.position;
		var targetLocalPosition = target.transform.localPosition;

		card.transform.DOKill();
		
		if (!target.TryGetComponent<SpecialCardSlot>(out var special)) {

			_placedCards[_slotInstances[target]] = card;
			card.transform.SetParent(CardFieldTransform, true);

		}
		else {

			_specialPlacedCards.Add(card);

			card.transform.SetParent(SpecialSlotField, true);
			special.BaseCard.AddCardOnSpecialSlot(card, special);
			special.PlacedCard = card;

		}


		SpawnSpecialSlots(card);

		CalculateSlotPositions();
		RedrawSlots();

		RescaleAndMove();

		card.PlayPlaceSound();

		card.transform.position = releasedWorldPosition;

		card.transform
			.DOLocalMove(targetLocalPosition, _cardPlacingDuration)
			.SetEase(_cardPlacingEase);

		card.transform
			.DOScale(Vector3.one, _cardPlacingDuration)
			.SetEase(_cardPlacingEase);

		card.transform
			.DORotate(target.transform.eulerAngles, _cardPlacingDuration)
			.SetEase(_cardPlacingEase);

		CardsChanged?.Invoke();

		UpdateStatus();

		return true;

	}

	public void UpdateStatus() {
		
		FinalBaseDamage = TotalCards.Sum(card => card.CalculateDamage());
		FinalMultiplier = TotalCards.Sum(card => card.CalculateAdditionalMultiplier()) + 1f;
		FinalMultiplier += CardHandDetector.Instance.CurrentMatches
			.Select(match => match.Bonus)
			.Sum();

		CardStatusTextDisplay.Instance.UpdateBaseDamage(FinalBaseDamage);
		CardStatusTextDisplay.Instance.UpdateMultiplier(FinalMultiplier);

	}

	public void SpawnSpecialSlots(Card card) {

		foreach (var slot in card.SpecialCardSlots) {
			
			GameObject instance = Instantiate(_specialSlotPrefab);
			instance.transform.SetParent(_specialSlotField);
			instance.transform.localScale = Vector3.one;

			var image = instance.GetComponent<Image>();
			image.color = new Color(0f, 0f, 0f, 0f);
			image.DOColor(new Color(1f, 1f, 1f, 0.3f), 0.2f).SetEase(Ease.OutExpo);

			_specialSlotInstances.Add(image, slot);

		}

	}

	public void Shoot() {

		if (_placedCards.Count + _specialPlacedCards.Count == 0)  return;

		var multiplier = FinalMultiplier + CardHandDetector.Instance.CurrentMatches.Sum(m => m.Bonus);
		CardEffect effect = new();

		foreach (var card in TotalCards) {
			effect += card.Effect;
		}
		
		GameObject parent = new("CardBullet");
		parent.transform.position = (_cardField.transform as RectTransform).GetGlobalBounds().center;

		foreach (var card in TotalCards) {
			card.GetComponent<CardAnimator>().RemoveAngle();
			card.transform.SetParent(parent.transform, false);
		}

		OnCardThrow.Invoke(new(
			FinalBaseDamage * multiplier,
			1f,
			parent,
			effect
		));

		Initialize();

	}

    private void OnReset()  {

        _placedCards.Clear();

        CalculateSlotPositions();
        RedrawSlots();
        CardsChanged?.Invoke();

		RescaleAndMove();
		Initialize();

		UpdateStatus();

    }

	private void Initialize() {

		FinalBaseDamage = 0f;
		FinalMultiplier = 1f;

		_placedCards.Clear();
		_specialPlacedCards.Clear();

		ClearSpecialSlots();

		CalculateSlotPositions();
		RedrawSlots();

		RescaleAndMove();

		CardStatusTextDisplay.Instance.UpdateBaseDamage(0f);
		CardStatusTextDisplay.Instance.UpdateMultiplier(1f);

	}

	private void ClearSpecialSlots() {

		foreach (var image in _specialSlotInstances.Keys) {

			if (image == null) {
				continue;
			}

			image.DOKill();
			image.gameObject.SetActive(false);
			Destroy(image.gameObject);

		}

		_specialSlotInstances.Clear();

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

        foreach (GameObject instance in _slotInstances.Keys) {
			instance.SetActive(false);
			Destroy(instance);
		}

        _slotInstances.Clear();

        foreach (Vector2Int position in _activeSlots) {

            GameObject instance = Instantiate(_slotPrefab, _slotField, false);
            instance.transform.localPosition = _gridSize * position;

            _slotInstances[instance] = position;

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

		SyncSpecialSlotTransforms();

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

			SyncSpecialSlotTransforms();

		}

	}

	private void SyncSpecialSlotTransforms() {

		foreach (var (image, slot) in _specialSlotInstances) {

			if (image == null ||
				slot == null) {
				continue;
			}

			image.transform.SetPositionAndRotation(
				slot.transform.position,
				slot.transform.rotation
			);

		}

	}


	private void OnDrawGizmos() {

		var contentBound = GetContentBounds();
		Gizmos.color = Color.skyBlue;
		Gizmos.DrawWireCube(contentBound.center, contentBound.size);

	}

}
