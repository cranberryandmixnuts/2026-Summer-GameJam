public class CreditCard : Card {

	private void Start() {
		AddEffect(new CardEffect() {
			HealLevel = 20
		});
	}

}