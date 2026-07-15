using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public abstract class Card : MonoBehaviour,
	IPointerEnterHandler,
	IPointerExitHandler,
	IPointerDownHandler,
	IPointerUpHandler {

	//======================================================================| Fields

	[SerializeField]
	private CardBaseStatus _baseStatus;

	private const float _slotReactiveRange = 5f;
	private GameObject _curentTargetSlot;
	protected readonly Dictionary<int, Card> _cardOnSpecialSlot = new();

	//======================================================================| Properties

	public CardBaseStatus BaseStatus => _baseStatus;
	public bool IsHovered { get; private set; }
	public bool IsGrabed { get; private set; }

	public int? PreviousIndex { get; set; } = null;
	public GameObject AttachedSlot { get; private set; } = null;

	public CardEffect Effect { get; private set; } = new();

	public virtual IReadOnlyList<SpecialCardSlot> SpecialCardSlots => Array.Empty<SpecialCardSlot>();

	//======================================================================| Events

	public event Action<Card> OnUpdate;

	//======================================================================| Unity Methods

	protected virtual void Update() {

		var currentPosition = transform.position;

		if (IsGrabed) {

			currentPosition = Camera.main
				.ScreenToWorldPoint(Mouse.current.position.ReadValue().ToVector3WithZ(100f))
				.WithZ(transform.position.z);

			transform.position = currentPosition;

			_curentTargetSlot = GetAttachSlot();

		}

		OnUpdate?.Invoke(this);
		
		foreach (var (index, card) in _cardOnSpecialSlot) {
			card.transform.SetPositionAndRotation(
				SpecialCardSlots[index].transform.position,
				SpecialCardSlots[index].transform.rotation
			);
		}

	}

	//======================================================================| Methods

	public virtual float CalculateDamage() => _baseStatus.BaseDamage;
	public virtual float CalculateAdditionalMultiplier() => _baseStatus.AdditionalMultiplier;

	public void PlayDrawSound() {
		AudioManager.Instance.PlayOneShotSFX("CardDraw", gameObject);
	}

	public void PlayHoverSound() {
        AudioManager.Instance.PlayOneShotSFX("CardHovered", gameObject);
    }

	public void PlayPlaceSound() {
        AudioManager.Instance.PlayOneShotSFX("CardPlace", gameObject);
    }

	public void AddCardOnSpecialSlot(Card target, int index) {
		_cardOnSpecialSlot.Add(index, target);
	}

	public void AddCardOnSpecialSlot(Card target, SpecialCardSlot slot) {
		_cardOnSpecialSlot.Add(SpecialCardSlots.IndexOf(slot), target);
	}

	public void AddEffect(CardEffect effect) {
		Effect += effect;
	}

	public void OnPointerEnter(PointerEventData eventData) {

		if (AttachedSlot != null) return;
		IsHovered = true;

	}

	public void OnPointerExit(PointerEventData eventData) {
		if (!IsGrabed)
			IsHovered = false;
	}

	public void OnPointerDown(PointerEventData eventData) {
		if (AttachedSlot != null) return;
		IsGrabed = true;
	}

	public void OnPointerUp(PointerEventData eventData) {

		if (AttachedSlot != null)
			return;

		IsGrabed = false;
		IsHovered = false;

		if (_curentTargetSlot != null) {

			AttachedSlot = _curentTargetSlot;

			PlayerHand.Instance.RemoveCard(this);
			CardField.Instance.PlaceCard(AttachedSlot, this);

		}

	}

	private GameObject GetAttachSlot() {

		var slots = CardField.Instance.SlotInstances.Keys
			.Concat(CardField.Instance.SpecialSlotInstances
				.Where(s => s.PlacedCard == null)
				.Select(s => s.gameObject)
			)
			.Where(obj => Vector2.Distance(obj.transform.position, transform.position) <= _slotReactiveRange);

		if (!slots.Any()) {
			return null;
		}

		var minDistance = float.MaxValue;
		GameObject result = null;

		foreach (var slot in slots) {
			
			var distance = Vector2.Distance(slot.transform.position, transform.position);

			if (distance < minDistance) {
				minDistance = distance;
				result = slot;
			}

		}

		return result;

	}


}