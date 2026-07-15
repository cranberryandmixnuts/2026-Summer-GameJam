public class DrawManager : SingletonBehaviour<DrawManager, SceneScope> {

	//======================================================================| Unity Methods

	private void OnEnable() {
		DrawTimer.OnDrawTiming += DrawAny;
	}

	private void OnDisable() {
		DrawTimer.OnDrawTiming -= DrawAny;
	}

	//======================================================================| Methods

	private void DrawAny() {
		DrawProcess(CardProvider.Instance.GetAnyCardInstance());
	}

	private void DrawAnyNormal() {
		DrawProcess(CardProvider.Instance.GetAnyNormalCardInstance());
	}

	private void DrawAnySpecial() {
		DrawProcess(CardProvider.Instance.GetAnySpecialCardInstance());
	}

	private void DrawProcess(Card card) {

		PlayerHand.Instance.AddCard(card);

	}

}