public class DrawManager : SingletonBehaviour<DrawManager, SceneScope> {

	//======================================================================| Unity Methods

	private void OnEnable() {
		DrawTimer.OnDrawTiming += DrawAnyNormal;
	}

	private void OnDisable() {
		DrawTimer.OnDrawTiming -= DrawAnyNormal;
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