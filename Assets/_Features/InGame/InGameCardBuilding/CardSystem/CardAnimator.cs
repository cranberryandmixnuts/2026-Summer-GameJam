using UnityEngine;

public class CardAnimator : MonoBehaviour {

	//======================================================================| Fields

	[SerializeField]
	private Transform _movementTiltingAnchore;
	
	[Header("Movement Tilt")]
	[SerializeField]
	private float _tiltBySpeedMultiplier = 7f;

	[SerializeField]
	private float _maxTiltAngle = 20f;

	[SerializeField]
	private float _tiltLerpFactor = 8f;

	private Vector3 _previousPosition;
	private Quaternion _initialFrontRotation;

	//======================================================================| Unity Methods 
	
	private void Awake() {

		_previousPosition = transform.position;
		_initialFrontRotation = _movementTiltingAnchore.transform.localRotation;

	}

	private void Update() {
		
		var currentPosition = transform.position;

		UpdateMovementTilt(currentPosition);
		_previousPosition = currentPosition;

	}

	//======================================================================| Methods

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

		_movementTiltingAnchore.transform.localRotation = Quaternion.Slerp(
			_movementTiltingAnchore.transform.localRotation,
			targetRotation,
			lerpAmount
		);

	}

}