using UnityEngine;
using UnityEngine.InputSystem;

public class ShootingButton : MonoBehaviour {

	private void Update() {
		if (Keyboard.current.spaceKey.wasPressedThisFrame) {
			CardField.Instance.Shoot();
		}
	}

}
