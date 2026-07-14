using UnityEngine;

public static class RectTransformExtensions {

	public static Bounds GetWorldBounds(this RectTransform rectTransform) {

		var corners = new Vector3[4];
		rectTransform.GetWorldCorners(corners);

		var bounds = new Bounds(corners[0], Vector3.zero);

		for (int i = 1; i < corners.Length; i++) {
			bounds.Encapsulate(corners[i]);
		}

		return bounds;

	}

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