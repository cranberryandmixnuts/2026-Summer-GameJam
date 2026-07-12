using UnityEngine;
using UnityEngine.InputSystem;

public class CardDrawTest : MonoBehaviour {

	private void Update() {
		
		if (Keyboard.current.insertKey.wasPressedThisFrame) {
			PlayerHand.Instance.AddCard(CardProvider.Instance.GetAnyCardInstance());
		}

	}

}