using System.Linq;
using UnityEngine.UI;

public class CardFireEffect : CardEffect {

	public CardFireEffect() {
		FireLevel = 1;
	}

	public override void OnAttached(Card card) {

		var images = card.GetComponentsInChildren<Image>().ToList();

		foreach (var image in images) {
			image.material = EffectSettingHolder.Setting.FireMaterial;
		}

	}

}