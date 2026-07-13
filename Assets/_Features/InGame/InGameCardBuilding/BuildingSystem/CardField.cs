using System.Collections.Generic;
using UnityEngine;

public class CardField : SingletonBehaviour<CardField, SceneScope> {

	//======================================================================| Fields

	[SerializeField]
	private GameObject _slotPrefab;

	[SerializeField]
	private Vector2 _gridSize;

	private readonly Dictionary<Vector2Int, Card> _placedCard = new();
	private readonly HashSet<Vector2Int> _activeSlots = new();

	private readonly Dictionary<Vector2Int, GameObject> _slotInstances = new();

	//======================================================================| Properties

	public IEnumerable<Vector2Int> ActiveSlots => _activeSlots;
	public IReadOnlyDictionary<Vector2Int, GameObject> SlotInstances => _slotInstances;

	//======================================================================| Unity Methods

	private void Start() {
		CalculateSlotPosition();
		RedrawSlots();
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

		_placedCard[position] = card;

		CalculateSlotPosition();
		RedrawSlots();

		return true;

	}

	private void OnReset() {
		_placedCard.Clear();
		_activeSlots.Clear();
		_slotInstances.Clear();
	}

	private void CalculateSlotPosition() {
		
		_activeSlots.Clear();
		
		if (_placedCard.Count == 0) {
			_activeSlots.Add(new(0, 0));
			print(_activeSlots.Count);
			return;
		}

		foreach (var (position, card) in _placedCard) {
		
			if (card.BaseStatus.IsBlockingAttachment) continue;

			int[] dx = { 0, 0, -1, 1 };
			int[] dy = { 1, -1, 0, 0 };

			for (int d = 0; d < 4; d++) {
	
				Vector2Int slotPosition = new(
					position.x + dx[d],
					position.y + dy[d]
				);

				if (_placedCard.ContainsKey(slotPosition)) continue;

				_activeSlots.Add(slotPosition);

			}

		}

	}

	private void RedrawSlots() {

		foreach (var (_, instance) in _slotInstances) {
			Destroy(instance);
		}

		_slotInstances.Clear();

		foreach (var position in _activeSlots) {

			print(position);

			var instance = Instantiate(_slotPrefab);
			instance.transform.SetParent(transform, false);
			instance.transform.localPosition = _gridSize * position;

			_slotInstances[position] = instance;

		}

	}

}