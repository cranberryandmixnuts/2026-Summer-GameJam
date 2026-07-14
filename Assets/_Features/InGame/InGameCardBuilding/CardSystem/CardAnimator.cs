using UnityEngine;
using UnityEngine.InputSystem;

public class CardAnimator : MonoBehaviour {

	//======================================================================| Fields

	[SerializeField]
	private Transform _movementTiltingAnchore;

	[SerializeField]
	private Transform _mouseTiltingAnchore;
	
	[Header("Movement Tilt")]
	[SerializeField]
	private float _tiltBySpeedMultiplier = 7f;

	[SerializeField]
	private float _maxTiltAngle = 20f;

	[SerializeField]
	private float _tiltLerpFactor = 8f;

	[Header("Mouse Tilt")]
	[SerializeField]
	private float _mouseTiltMultiplier = 8f;

	private Card _card;

	private Vector3 _previousPosition;

	//======================================================================| Unity Methods 
	
	private void Awake() {

		_card = GetComponent<Card>();
		_previousPosition = transform.position;

	}

	private void Update() {
		
		var currentPosition = transform.position;

		UpdateMovementTilt(currentPosition);
		_previousPosition = currentPosition;

		UpdateMouseTilt();

	}

	//======================================================================| Methods

	private void UpdateMouseTilt() {

		Quaternion targetRotation;

		if (_card.IsHovered) {

			var bound = (transform as RectTransform).GetWorldBounds();
			var maxDistance = bound.size.WithZ(0).magnitude;

			Vector2 mousePosition = Camera.main
				.ScreenToWorldPoint(Mouse.current.position.ReadValue().ToVector3WithZ(100f));

			var position = (bound.center.ToVector2WithoutZ() - mousePosition) / maxDistance;

			Vector3 targetEulerAngle = new(
				position.y * _mouseTiltMultiplier,
				-position.x * _mouseTiltMultiplier,
				0f
			); 

			targetRotation = Quaternion.Euler(targetEulerAngle);

		}
		else {
			targetRotation = Quaternion.identity;
		}

			
		var lerpAmount =
			1f - Mathf.Exp(-_tiltLerpFactor * Time.deltaTime);

		_mouseTiltingAnchore.transform.localRotation = Quaternion.Slerp(
			_mouseTiltingAnchore.transform.localRotation,
			targetRotation,
			lerpAmount
		);


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

		var targetRotation = Quaternion.Euler(targetEulerAngle);

		var lerpAmount =
			1f - Mathf.Exp(-_tiltLerpFactor * Time.deltaTime);

		_movementTiltingAnchore.transform.localRotation = Quaternion.Slerp(
			_movementTiltingAnchore.transform.localRotation,
			targetRotation,
			lerpAmount
		);

	}

}