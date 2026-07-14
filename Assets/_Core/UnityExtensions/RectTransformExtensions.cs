using UnityEngine;

public static class RectTransformExtensions {

	public static Bounds GetGlobalBounds(this RectTransform rectTransform) {

		var rects = rectTransform.GetComponentsInChildren<RectTransform>(true);

		bool initialized = false;
		Bounds bounds = default;

		Vector3[] corners = new Vector3[4];

		foreach (var rect in rects) {

			rect.GetWorldCorners(corners);

			foreach (var corner in corners) {

				if (!initialized) {
					bounds = new Bounds(corner, Vector3.zero);
					initialized = true;
				}
				else {
					bounds.Encapsulate(corner);
				}

			}

		}

		return bounds;

	}

}