using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerInputReader : MonoBehaviour
{
    [SerializeField, Required] private InputActionReference moveAction;

    public Vector2 Movement => moveAction.action.ReadValue<Vector2>();

    private void OnEnable() => moveAction.action.Enable();

    private void OnDisable() => moveAction.action.Disable();
}
