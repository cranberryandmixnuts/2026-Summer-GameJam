using System;

public class CardBuildingManager : SingletonBehaviour<CardBuildingManager, SceneScope> {

	//======================================================================| Event

	public static event Action OnRestarted;

	//======================================================================| Methods

	public void Restart() {
		OnRestarted?.Invoke();
	}

}