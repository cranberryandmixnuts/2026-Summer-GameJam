using UnityEngine;

public class NormalCard : Card {

	//======================================================================| Fields

	[SerializeField]
	private CardPatternType _pattern;

	[SerializeField]
	private int _number;

	//======================================================================| Properties

	public CardPatternType Pattern => _pattern;
	public int Number => _number;

	//======================================================================| Methods

	public override float CalculateDamage() {
		return base.CalculateDamage() * (1 + _number / 13f);
	}

}