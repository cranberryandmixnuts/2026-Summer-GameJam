using DG.Tweening;
using TMPro;

public class CardStatusTextDisplay : SingletonBehaviour<CardStatusTextDisplay, SceneScope> {

	//======================================================================| Fields

	private TMP_Text _text;

	private float _baseDamage;
	private float _multiplier = 1f;

	private Tween _baseDamageTween;
	private Tween _multiplierTween;

	//======================================================================| Unity Methods

	protected override void SingletonAwake() {
		_text = GetComponent<TMP_Text>();
	}

	private void Update() {
		
		_text.text =
			$"데미지: {_baseDamage:0.00}\n" +
			$"배수: {_multiplier:0.00}\n";

	}

	//======================================================================| Methods

	public void UpdateBaseDamage(float value) {
		_baseDamageTween?.Kill();
		_baseDamageTween = DOTween.To(
			() => _baseDamage,
			x => _baseDamage = x,
			value, 0.5f
		).SetEase(Ease.OutQuad);
	}

	public void UpdateMultiplier(float value) {
		_multiplierTween?.Kill();
		_multiplierTween = DOTween.To(
			() => _multiplier,
			x => _multiplier = x,
			value, 0.5f
		).SetEase(Ease.OutQuad);
	}

}