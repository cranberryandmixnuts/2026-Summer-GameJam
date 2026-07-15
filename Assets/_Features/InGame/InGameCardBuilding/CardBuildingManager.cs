using System;
using System.Collections;

public class CardBuildingManager : SingletonBehaviour<CardBuildingManager, SceneScope> {

	//======================================================================| Event

	public static event Action OnRestarted;

	//======================================================================| Unity Methods

	private void Start() {
		
		StartCoroutine(WaitAndRun());

		IEnumerator WaitAndRun() {
			yield return null;
			Restart();
		}
		
	}

	//======================================================================| Methods

	public void Restart() {
		OnRestarted?.Invoke();
	}

}