public class PoisonCard : Card {

	private void Start() {
		AddEffect(new CardEffect() {
			IsPoison = true
		});
	}

}