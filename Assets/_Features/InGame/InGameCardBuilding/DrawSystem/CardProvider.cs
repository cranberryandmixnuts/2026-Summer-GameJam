using UnityEngine;

public class CardProvider : SingletonBehaviour<CardProvider, SceneScope> {

	//======================================================================| Fields

	[SerializeField]
	private CardDatabase _database;

	//======================================================================| Unity Methods

	private void OnEnable() => CardBuildingManager.OnRestarted += _database.Initialize;
	private void OnDisable() => CardBuildingManager.OnRestarted -= _database.Initialize;

	//======================================================================| Methods

	public Card GetAnyCardInstance() {
		int index = Random.Range(0, _database.Cards.Count);
		return Instantiate(_database.Cards[index]);
	}

	public Card GetAnyNormalCardInstance() {
		int index = Random.Range(0, _database.NormalCards.Count);
		return Instantiate(_database.NormalCards[index]);
	}

	public Card GetAnySpecialCardInstance() {
		int index = Random.Range(0, _database.SpecialCards.Count);
		return Instantiate(_database.SpecialCards[index]);
	}

}