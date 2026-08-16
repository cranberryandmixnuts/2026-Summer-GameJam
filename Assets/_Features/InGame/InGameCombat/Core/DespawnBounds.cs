using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class DespawnBounds : MonoBehaviour
{
    [SerializeField, Required] private BoxCollider2D area;

    public bool Overlaps(Collider2D target) => area.bounds.Intersects(target.bounds);

    private void Reset()
    {
        area = GetComponent<BoxCollider2D>();
        area.isTrigger = true;
    }
}
