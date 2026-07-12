using UnityEngine;

[DisallowMultipleComponent]
public sealed class DestroyBelowWorldY : MonoBehaviour
{
    [SerializeField] private float destroyY = -8f;

    private void Update()
    {
        if (transform.position.y < destroyY) Destroy(gameObject);
    }
}
