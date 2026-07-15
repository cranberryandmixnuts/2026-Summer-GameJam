using DG.Tweening;
using TMPro;

public class CardStatusTextDisplay : SingletonBehaviour<CardStatusTextDisplay, SceneScope> {

	//======================================================================| Fields

	private TMP_Text _text;

	private float _baseDamage;
	private float _multiplier;

	private Tween _baseDamageTween;
	private Tween _multiplierTween;

	//======================================================================| Unity Methods

	protected override void SingletonAwake() {
		_text = GetComponent<TMP_Text>();
	}

	//======================================================================| Methods

	public void UpdateBaseDamage(float value) {
		_baseDamageTween?.Kill();
		_baseDamageTween = DOTween.To(
			() => _baseDamage,
			x => _baseDamage = x,
			value, 0.25f
		).SetEase(Ease.OutExpo);
	}

	public void UpdateMultiplier(float value) {
		_multiplierTween?.Kill();
		_multiplierTween = DOTween.To(
			() => _multiplier,
			x => _multiplier = x,
			value, 0.25f
		).SetEase(Ease.OutExpo);
	}

}