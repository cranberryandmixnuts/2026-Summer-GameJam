using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class ButtonHotkey : MonoBehaviour
{
    [SerializeField, Required] private Button button;
    [SerializeField] private Key key = Key.Space;

    private void Update()
    {
        if (key == Key.None)
            return;

        if (!Keyboard.current[key].wasPressedThisFrame)
            return;

        if (!button.isActiveAndEnabled || !button.interactable)
            return;

        button.onClick.Invoke();
    }
}