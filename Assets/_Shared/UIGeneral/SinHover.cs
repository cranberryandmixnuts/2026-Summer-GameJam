using Sirenix.OdinInspector;
using UnityEngine;

public sealed class SinHover : BaseBehaviour
{
    [SerializeField]
    private Transform target;

    [SerializeField]
    [MinValue(0f)]
    private float amplitude = 0.5f;

    [SerializeField]
    [MinValue(0f)]
    [SuffixLabel("회/초")]
    private float frequency = 1f;

    private Vector3 previousOffset;
    private float elapsedTime = 0;

    private void OnEnable()
    {
        previousOffset = Vector3.zero;
    }

    private void LateUpdate()
    {
        elapsedTime += Time.deltaTime;

        float angle = elapsedTime * frequency * Mathf.PI * 2f;
        Vector3 offset = Vector3.up * Mathf.Sin(angle) * amplitude;

        target.localPosition = target.localPosition - previousOffset + offset;
        previousOffset = offset;
    }

    private void OnDisable()
    {
        if (target == null)
            return;

        target.localPosition -= previousOffset;
        previousOffset = Vector3.zero;
    }
}