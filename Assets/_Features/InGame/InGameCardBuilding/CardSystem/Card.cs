using UnityEngine;

public abstract class Card : MonoBehaviour {

	//======================================================================| Fields

	[SerializeField]
	private CardBaseStatus _baseStatus;

	//======================================================================| Properties

	public CardBaseStatus BaseStatus => _baseStatus;
	public bool IsHovered { get; private set; }

	//======================================================================| Methods

	public virtual float CalculateDamage() {
		return _baseStatus.BaseDamage;
	}

	public virtual float CalculateAdditionalMultiplier() {
		return _baseStatus.AdditionalMultiplier;
	}

}