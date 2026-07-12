using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CombatScreenCenterAlignment : MonoBehaviour {

	//======================================================================| Fields
	 
	[SerializeField]
	private Camera _mainCamera;
	private Vector2 _targetPosition;

	//======================================================================| Properties

	private RectTransform RectTransform => transform as RectTransform;

	//======================================================================| Unity Methods

	private void OnRectTransformDimensionsChange() {
		
		var corners = new Vector3[4];
		RectTransform.GetWorldCorners(corners);

		var height = corners[1].y - corners[0].y;

		var targetWidth = height / 16 * 9;
		var currentCenterX = corners[0].x + targetWidth / 2f;

		_targetPosition = new Vector3(
			transform.position.x - currentCenterX,
			transform.position.y,
			transform.position.z
		);

	}

	private void LateUpdate() {

		_mainCamera.transform.position =
			_targetPosition.ToVector3WithZ(_mainCamera.transform.position.z);

	}

}
