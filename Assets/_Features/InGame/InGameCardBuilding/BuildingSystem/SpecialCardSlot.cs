using UnityEngine;

public class SpecialCardSlot : MonoBehaviour {

	public Card BaseCard { get; private set; }
	public int Index { get; private set; }

	public Card PlacedCard { get; set; } = null;

	public static SpecialCardSlot NewSlot(Card baseCard, int index) {
		var result = new GameObject("SpecialSlot").AddComponent<SpecialCardSlot>();
		result.transform.SetParent(baseCard.transform);
		result.BaseCard = baseCard;
		result.Index = index;
		return result;
	}

}