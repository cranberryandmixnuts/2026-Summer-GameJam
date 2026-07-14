using DG.Tweening;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public abstract class Card : MonoBehaviour,
	IPointerEnterHandler,
	IPointerExitHandler,
	IPointerDownHandler,
	IPointerUpHandler
{

	//======================================================================| Fields

	[SerializeField]
	private CardBaseStatus _baseStatus;

	[SerializeField]
	private Image _frontImage;

	[Space]
	[SerializeField]
	private float _slotReactiveRange;

	private Vector2Int? _curentTargetSlot;

	//======================================================================| Properties

	public CardBaseStatus BaseStatus => _baseStatus;
	public bool IsHovered { get; private set; }
	public bool IsGrabed { get; private set; }

	public int? PreviousIndex { get; set; } = null;
	public Vector2Int? AttachedSlot { get; private set; } = null;

	//======================================================================| Unity Methods

	private void Update() {

		if (IsGrabed) {

			transform.position = Camera.main
				.ScreenToWorldPoint(Mouse.current.position.ReadValue())
				.WithZ(transform.position.z);

			if (TryGetAttachSlot(out var slotPosition)) {
				print(true);
				_curentTargetSlot = slotPosition;
			}
			else {
				print(false);
				_curentTargetSlot = null;
			}

		}

	}

	//======================================================================| Methods

	public virtual float CalculateDamage() => _baseStatus.BaseDamage;
	public virtual float CalculateAdditionalMultiplier() => _baseStatus.AdditionalMultiplier;

	public void OnPointerEnter(PointerEventData eventData) {
		if (AttachedSlot != null) return;
		IsHovered = true;
	}

	public void OnPointerExit(PointerEventData eventData) {
		if (!IsGrabed) IsHovered = false;
	}

	public void OnPointerDown(PointerEventData eventData) {
		if (AttachedSlot != null) return;
		IsGrabed = true;
	}

	public void OnPointerUp(PointerEventData eventData) {

		if (AttachedSlot != null) return;

		IsGrabed = false;
		IsHovered = false;

		if (_curentTargetSlot.HasValue) {

			AttachedSlot = _curentTargetSlot;

			transform.SetParent(CardField.Instance.CardFieldTransform);

			transform
				.DOLocalMove(CardField.Instance.SlotInstances[_curentTargetSlot.Value].transform.localPosition, 0.2f);

				
			PlayerHand.Instance.RemoveCard(this);
			CardField.Instance.PlaceCard(_curentTargetSlot.Value, this);

		}

	}

	private bool TryGetAttachSlot(out Vector2Int slotPosition) {

		var slots = CardField.Instance.SlotInstances;
		slots.Where(pair => Vector2.Distance(pair.Value.transform.position, transform.position) <= _slotReactiveRange);

		if (!slots.Any()) {
			slotPosition = Vector2Int.zero;
			return false;
		}

		float minDistance = float.MaxValue;
		slotPosition = Vector2Int.zero;

		foreach (var (position, obj) in CardField.Instance.SlotInstances) {
			
			var distance = Vector2.Distance(obj.transform.position, transform.position);

			if (minDistance > distance) {
				minDistance = distance;
				slotPosition = position;
			}

		}

		return true;

	}

}