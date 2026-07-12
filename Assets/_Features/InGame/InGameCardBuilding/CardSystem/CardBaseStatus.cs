using UnityEngine;

[CreateAssetMenu(
	fileName = nameof(CardBaseStatus),
	menuName = "Cards/" + nameof(CardBaseStatus)
)]

public class CardBaseStatus : ScriptableObject {

	//======================================================================| Fields

	[SerializeField]
	private float _baseDamage;

	[SerializeField]
	private float _additionalMultiplier;

	[SerializeField]
	private bool _isJoker;

	[SerializeField]
	private bool _isSpecial;

	//======================================================================| Properties

	public float BaseDamage => _baseDamage;
	public float AdditionalMultiplier => _additionalMultiplier;
	public bool IsJoker => _isJoker;
	public bool IsSpecial => _isSpecial;

}