using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public abstract class Card : MonoBehaviour,
	IPointerEnterHandler,
	IPointerExitHandler,
	IPointerDownHandler,
	IPointerUpHandler {

	//======================================================================| Fields

	[SerializeField]
	private CardBaseStatus _baseStatus;

	[SerializeField]
	private Image _frontImage;

	[Space]
	[SerializeField]
	private float _slotReactiveRange;

	[Header("Movement Tilt")]
	[SerializeField]
	private float _tiltBySpeedMultiplier = 1f;

	[SerializeField]
	private float _maxTiltAngle = 15f;

	[SerializeField]
	private float _tiltLerpFactor = 12f;

	private Vector3 _previousPosition;
	private Quaternion _initialFrontRotation;

	private Vector2Int? _curentTargetSlot;

	//======================================================================| Properties

	public CardBaseStatus BaseStatus => _baseStatus;
	public bool IsHovered { get; private set; }
	public bool IsGrabed { get; private set; }

	public int? PreviousIndex { get; set; } = null;
	public Vector2Int? AttachedSlot { get; private set; } = null;

	//======================================================================| Unity Methods

	private void Awake() {

		_previousPosition = transform.position;
		_initialFrontRotation = _frontImage.transform.localRotation;

	}

	private void Update() {

		var currentPosition = transform.position;

		if (IsGrabed) {

			currentPosition = Camera.main
				.ScreenToWorldPoint(Mouse.current.position.ReadValue())
				.WithZ(transform.position.z);

			transform.position = currentPosition;

			if (TryGetAttachSlot(out var slotPosition)) {
				_curentTargetSlot = slotPosition;
			}
			else {
				_curentTargetSlot = null;
			}

		}

		UpdateMovementTilt(currentPosition);

		_previousPosition = currentPosition;

	}

	//======================================================================| Methods

	public virtual float CalculateDamage() => _baseStatus.BaseDamage;
	public virtual float CalculateAdditionalMultiplier() => _baseStatus.AdditionalMultiplier;

	public void OnPointerEnter(PointerEventData eventData) {
		if (AttachedSlot != null)
			return;
		IsHovered = true;
	}

	public void OnPointerExit(PointerEventData eventData) {
		if (!IsGrabed)
			IsHovered = false;
	}

	public void OnPointerDown(PointerEventData eventData) {
		if (AttachedSlot != null)
			return;
		IsGrabed = true;
	}

	public void OnPointerUp(PointerEventData eventData) {

		if (AttachedSlot != null)
			return;

		IsGrabed = false;
		IsHovered = false;

		if (_curentTargetSlot.HasValue) {

			AttachedSlot = _curentTargetSlot;

			PlayerHand.Instance.RemoveCard(this);
			CardField.Instance.PlaceCard(AttachedSlot.Value, this);

		}

	}

	private bool TryGetAttachSlot(out Vector2Int slotPosition) {

		var slots = CardField.Instance.SlotInstances
			.Where(pair => Vector2.Distance(pair.Value.transform.position, transform.position) <= _slotReactiveRange);

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

	private void UpdateMovementTilt(Vector3 currentPosition) {

		if (Time.deltaTime <= Mathf.Epsilon) return;

		var velocity = (currentPosition - _previousPosition) / Time.deltaTime;

		var targetEulerAngle = new Vector3(
			velocity.y * _tiltBySpeedMultiplier,
			-velocity.x * _tiltBySpeedMultiplier,
			0f
		);

		targetEulerAngle.x = Mathf.Clamp(
			targetEulerAngle.x,
			-_maxTiltAngle,
			_maxTiltAngle
		);

		targetEulerAngle.y = Mathf.Clamp(
			targetEulerAngle.y,
			-_maxTiltAngle,
			_maxTiltAngle
		);

		var targetRotation =
			_initialFrontRotation *
			Quaternion.Euler(targetEulerAngle);

		var lerpAmount =
			1f - Mathf.Exp(-_tiltLerpFactor * Time.deltaTime);

		_frontImage.transform.localRotation = Quaternion.Slerp(
			_frontImage.transform.localRotation,
			targetRotation,
			lerpAmount
		);

	}

}