using Sirenix.OdinInspector;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public sealed class CombatBounds2D : MonoBehaviour
{
    [SerializeField, Required] private BoxCollider2D area;

    public Vector2 Clamp(Vector2 currentRootPosition, Vector2 targetRootPosition, Collider2D movingCollider)
    {
        Bounds areaBounds = area.bounds;
        Bounds movingBounds = movingCollider.bounds;
        Vector2 centerOffset = (Vector2)movingBounds.center - currentRootPosition;
        Vector2 extents = movingBounds.extents;

        float x = Mathf.Clamp(
            targetRootPosition.x,
            areaBounds.min.x + extents.x - centerOffset.x,
            areaBounds.max.x - extents.x - centerOffset.x);
        float y = Mathf.Clamp(
            targetRootPosition.y,
            areaBounds.min.y + extents.y - centerOffset.y,
            areaBounds.max.y - extents.y - centerOffset.y);

        return new Vector2(x, y);
    }

    private void Reset()
    {
        area = GetComponent<BoxCollider2D>();
        area.isTrigger = true;
    }
}
