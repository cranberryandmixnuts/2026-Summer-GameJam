using System.Collections.Generic;
using UnityEngine;

public class PlayerHand : SingletonBehaviour<PlayerHand, SceneScope> {

	//======================================================================| Fields

	[SerializeField]
	private int _handCardLimit;

	[SerializeField]

	private readonly List<Card> _cards = new();

	//======================================================================| Properties

	public int HandCardLimit => _handCardLimit;
	public IReadOnlyList<Card> Cards => _cards;

	//======================================================================| Unity Methods

	public void AddCard(Card card) {
		_cards.Add(card);
	}

}