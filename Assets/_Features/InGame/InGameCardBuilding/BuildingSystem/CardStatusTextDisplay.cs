using DG.Tweening;
using System.Collections.Generic;
using TMPro;

public class CardStatusTextDisplay : SingletonBehaviour<CardStatusTextDisplay, SceneScope> {

	//======================================================================| Constants

	private readonly Dictionary<string, string> _textOfMatches = new() {

		{ "페어", "<color=White>페어</color>" },
		{ "페어+", "<color=#ffffff>페어</color><color=#FFCE7A>+</color>" },
		{ "트리플", "<color=#99CCFF>트리플</color>" },
		{ "트리플+", "<color=#99CCFF>트리플</color><color=#FFCE7A>+</color>" },
		{ "트리플++", "<color=#78A3FA>트리플</color><color=#FFCE7A>++</color>" },
		{ "포카드", "<color=#FFFF99>포카드</color>" },
		{ "포카드+", "<color=#FFFF99>포카드</color><color=#FFCE7A>+</color>" },
		{ "포카드++", "<color=#FFB47A>포카드</color><color=#FFCE7A>++</color>" },
		{ "포카드+++", "<color=#FF844F>포카드</color><color=#FFCE7A>+++</color>" },
		{ "플러시", "<color=#E5CCFF>플러시</color>" },
		{ "플러시+", "<color=#E5CCFF>플러시</color><color=#FFCE7A>++</color>" },
		{ "플러시++", "<color=#B296FF>플러시</color><color=#FFCE7A>++</color>" },
		{ "플러시+++", "<color=#8C75FF>플러시</color><color=#FFCE7A>++</color>" },
		{ "야추", "<color=#4F4FFF>야추</color>" },
		{ "풀하우스", "<color=#CCFFCC>풀하우스</color>" },
		{ "풀하우스+", "<color=#CCFFCC>풀하우스</color><color=#FFCE7A>+</color>" },
		{ "풀하우스++", "<color=#BAFF8C>풀하우스</color><color=#FFCE7A>++</color>" },
		{ "퍼펙트 풀하우스", "<color=#FFCE7A>퍼펙트</color><color=#BAFF8C>풀하우스</color>" },
		{ "퍼펙트 풀하우스+", "<color=#FFCE7A>퍼펙트</color><color=#BAFF8C>풀하우스</color><color=#FFCE7A>+</color>" },
		{ "스트레이트", "<color=#FFABAB>스트레이트</color>" },
		{ "스트레이트 플러시", "<color=#FF6B8E>스트레이트</color><color=#B296FF>플러시</color>" },
		{ "로열 스트레이트", "<color=#FFCE7A>로열</color><color=#FF6B8E>스트레이트</color>" },
		{ "로열 스트레이트 플러시", "<color=#FFCE7A>로열</color><color=#FF6B8E>스트레이트</color><color=#B296FF>플러시</color>" },

	};

	//======================================================================| Fields

	private TMP_Text _text;

	private float _baseDamage;
	private float _multiplier = 1f;

	private Tween _baseDamageTween;
	private Tween _multiplierTween;
	private readonly Dictionary<string, int> _matches = new();
	private string _matchText;

	//======================================================================| Unity Methods

	protected override void SingletonAwake() {
		_text = GetComponent<TMP_Text>();
	}

	private void Update() {
		
		_text.text =
			$"데미지: {_baseDamage:0.00}\n" +
			$"배수: {_multiplier:0.00}\n";

		if (!string.IsNullOrEmpty(_matchText)) {
			_text.text += "\n족보 보너스";
			_text.text += _matchText;
		}

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

	public void RemoveMatches() {
		_matchText = "";
	}

	public void UpdateMatches() {

		var matches = CardHandDetector.Instance.CurrentMatches;
		_matches.Clear();

		foreach (var matche in matches) {

			UnityEngine.Debug.Log(
				$"Rule Type: {matche.DisplayName}, Rule Id: {matche.Rule.Id}, Name: {matche.Rule.name}"
			);


			if (_matches.ContainsKey(matche.DisplayName)) {
				_matches[matche.DisplayName]++;
			}
			else {
				_matches[matche.DisplayName] = 1;
			}

		}

		_matchText = "";

		foreach (var (id, count) in _matches) {
			_matchText += $"\n  - {_textOfMatches[id]}";
			if (count == 1) continue;
			_matchText += $" ({count})";
		}

	}

}