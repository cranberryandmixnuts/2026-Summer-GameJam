using UnityEngine;

public static class RectTransformExtensions
{
    public static Bounds GetWorldBounds(this RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        Bounds bounds = new(corners[0], Vector3.zero);

        for (int i = 1; i < corners.Length; i++) bounds.Encapsulate(corners[i]);

        return bounds;
    }

    public static Bounds GetGlobalBounds(this RectTransform rectTransform)
    {
        RectTransform[] rects = rectTransform.GetComponentsInChildren<RectTransform>();
        bool initialized = false;
        Bounds bounds = default;
        Vector3[] corners = new Vector3[4];

        foreach (RectTransform rect in rects)
        {
            rect.GetWorldCorners(corners);

            foreach (Vector3 corner in corners)
            {
                if (!initialized)
                {
                    bounds = new Bounds(corner, Vector3.zero);
                    initialized = true;
                }
                else bounds.Encapsulate(corner);
            }
        }

        return bounds;
    }
}