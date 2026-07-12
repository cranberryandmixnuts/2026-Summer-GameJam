using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public sealed class FireButtonController : MonoBehaviour
{
    [SerializeField, Required] private Button button;
    [SerializeField, Required] private CombatBridge combatBridge;

    public bool Interactable
    {
        get => button.interactable;
        set => button.interactable = value;
    }

    private void OnEnable() => button.onClick.AddListener(HandleClicked);

    private void OnDisable() => button.onClick.RemoveListener(HandleClicked);

    private void HandleClicked() => combatBridge.RequestFire();

    private void Reset() => button = GetComponent<Button>();
}
