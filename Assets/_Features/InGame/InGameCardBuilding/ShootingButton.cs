using UnityEngine;
using UnityEngine.InputSystem;

public class ShootingButton : BaseBehaviour {

	private void Update() {
		if (Keyboard.current.spaceKey.wasPressedThisFrame) {
			CardField.Instance.Shoot();
		}
	}

}
