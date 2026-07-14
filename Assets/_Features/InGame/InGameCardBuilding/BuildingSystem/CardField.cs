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
    private Vector2 _gridSize;

    private readonly Dictionary<Vector2Int, Card> _placedCards = new();
    private readonly HashSet<Vector2Int> _activeSlots = new();
    private readonly Dictionary<Vector2Int, GameObject> _slotInstances = new();

	//======================================================================| Event

	public event Action CardsChanged;

	//======================================================================| Properties

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

    private void OnEnable() {
        CardBuildingManager.OnRestarted += OnReset;
    }

    private void OnDisable() {
        CardBuildingManager.OnRestarted -= OnReset;
    }

	//======================================================================| Methods

	public bool PlaceCard(Vector2Int position, Card card) {

        if (!_activeSlots.Contains(position)) return false;

        _placedCards[position] = card;

        CalculateSlotPositions();
        RedrawSlots();
        CardsChanged?.Invoke();

        return true;

    }

    private void OnReset()  {

        _placedCards.Clear();

        CalculateSlotPositions();
        RedrawSlots();
        CardsChanged?.Invoke();

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
			Destroy(instance);
		}

        _slotInstances.Clear();

        foreach (Vector2Int position in _activeSlots) {

            GameObject instance = Instantiate(_slotPrefab, _slotField, false);
            instance.transform.localPosition = _gridSize * position;

            _slotInstances[position] = instance;

        }

    }

}
