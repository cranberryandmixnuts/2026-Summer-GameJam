using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	//======================================================================| Fields

	[SerializeField]
	private CardBaseStatus _baseStatus;

	//======================================================================| Properties

	public CardBaseStatus BaseStatus => _baseStatus;
	public bool IsHovered { get; private set; }

	//======================================================================| Methods

	public virtual float CalculateDamage() => _baseStatus.BaseDamage;
	public virtual float CalculateAdditionalMultiplier() => _baseStatus.AdditionalMultiplier;

	public void OnPointerEnter(PointerEventData eventData) => IsHovered = true;
	public void OnPointerExit(PointerEventData eventData) => IsHovered = false;

}