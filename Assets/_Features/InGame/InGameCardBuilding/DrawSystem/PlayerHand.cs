using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerHand : SingletonBehaviour<PlayerHand, SceneScope> {

	//======================================================================| Fields

	[SerializeField]
	private int _handCardLimit;

	[Header("Display")]
	[SerializeField] private float _displayRadius;
	[SerializeField] private float _displayNormalInterval;
	[SerializeField] private float _displayMaxRange;

	[Space]
	[SerializeField] private float _hoverHeight;
	[SerializeField] private float _hoverScale;
	[SerializeField] private float _hoverAdditionalPosition;

	[Header("Animation")]
	[SerializeField] private float _hoverAnimationDuration;
	[SerializeField] private float _hoverAnimationSpreadingDuration;
	[SerializeField] private Ease _hoverAnimationEase;

	[Space]
	[SerializeField] private float _drawAnimationDuration;
	[SerializeField] private Ease _drawAnimationEase;

	private Card _previousHoveredCard = null;

	private readonly List<Card> _cards = new();
	private readonly Dictionary<Card, float> _cardPosition = new();

	private readonly HashSet<Tween> _killableTweens = new();

	//======================================================================| Properties

	public int HandCardLimit => _handCardLimit;
	public IReadOnlyList<Card> Cards => _cards;

	public int HoveredCardIndex { get; private set; } = -1;
	public Card HoveredCard { get; private set; } = null;

	//======================================================================| Unity Methods

	private void Update() {

		HoveredCard = _cards.FirstOrDefault(card => card.IsHovered);
		
		HoveredCardIndex = HoveredCard != null
			? _cards.IndexOf(HoveredCard)
			: -1;

		if (HoveredCard != _previousHoveredCard) {

			var startCard = HoveredCard == null
				? _previousHoveredCard
				: HoveredCard;

			CalculateCardPosition();
			MoveCards(HoveredCardIndex, _hoverAnimationSpreadingDuration / _cards.Count);

			if (HoveredCard != null) {
				HoveredCard.transform
					.DOScale(_hoverScale, _hoverAnimationDuration)
					.SetEase(_hoverAnimationEase);
			}

			if (_previousHoveredCard != null) {
				_previousHoveredCard.transform
					.DOScale(1f, _drawAnimationDuration)
					.SetEase(_drawAnimationEase);
			}

			_previousHoveredCard = HoveredCard;

		}

	}

	//======================================================================| Methods

	public void AddCard(Card card) {

		card.transform.SetParent(transform, false);

		if (card.PreviousIndex.HasValue) {
			_cards.Insert(card.PreviousIndex.Value, card);
		}
		else {
			_cards.Add(card);
		}

		_cardPosition.Add(card, default);

		CalculateCardPosition();
		MoveCards(_cards.Count - 1, 0f);

	}

	public void RemoveCard(Card card) {
		card.PreviousIndex = _cards.IndexOf(card);
		_cards.Remove(card);
		_cardPosition.Remove(card);
	}

	private void CalculateCardPosition() {

		if (_cards.Count == 1) {
			_cardPosition[_cards[0]] = 0f;
			return;
		}
	
		var range = Mathf.Min(
			_displayMaxRange,
			(_cards.Count - 1) * _displayNormalInterval
		);

		var rawRange = range;
		if (HoveredCard != null) range += _hoverAdditionalPosition;

		for (int i = 0; i < _cards.Count; i++) {

			var card = _cards[i];
			var position = i * rawRange / (_cards.Count - 1);

			if (HoveredCard != null) {

				if (i > HoveredCardIndex)
					position += _hoverAdditionalPosition;

				else if (i == HoveredCardIndex)
					position += _hoverAdditionalPosition / 2f;

			}

			_cardPosition[card] = position - range / 2f;

		}

	}

	private void MoveCards(int startPosition, float timeInterval) {
		
		foreach (var tween in _killableTweens) {
			tween.Kill();
		}

		_killableTweens.Clear();

		StartCoroutine(Routine());
		IEnumerator Routine() {

  			int leftIndex = startPosition;
			int rightIndex = startPosition;

			while (leftIndex >= 0 || rightIndex < _cards.Count) {
			
				if (leftIndex >= 0) {
					PlayAnimation(_cards[leftIndex]);
				}

				if (rightIndex < _cards.Count && rightIndex != leftIndex) {
					PlayAnimation(_cards[rightIndex]);
				}

				leftIndex--;
				rightIndex++;

				if (timeInterval != 0f)
					yield return new WaitForSeconds(timeInterval);

			}

			void PlayAnimation(Card card) {

				_killableTweens.Add(card.transform
					.DOLocalMove(GetCardPosition(card), _drawAnimationDuration)
					.SetEase(_drawAnimationEase)
				);

				_killableTweens.Add(card.transform
					.DOLocalRotate(Vector3.forward * GetCardAngle(card), _drawAnimationDuration)
					.SetEase(_drawAnimationEase)
				);

			}
		}

	}

	private Vector3 GetCardPosition(Card card) {
		
		var unitAngle = 1f / _displayRadius;
		var angle = -_cardPosition[card] * unitAngle + Mathf.PI / 2f;

		var position = _displayRadius * new Vector3(
			x: Mathf.Cos(angle),
			y: Mathf.Sin(angle)
		);

		if (card.IsHovered) {
			position += Vector3.up * _hoverHeight;
		}

		return position + Vector3.down * _displayRadius;
	}

	private float GetCardAngle(Card card) {
			
		if (card.IsHovered) return 0f;

		var unitAngle = 1f / _displayRadius;
		var angle = -_cardPosition[card] * unitAngle + Mathf.PI / 2f;

		var direction = new Vector2(
			x: Mathf.Cos(angle),
			y: Mathf.Sin(angle)
		);

		return Vector2.SignedAngle(Vector2.up, direction.normalized);

	}

	//======================================================================| Gizmos

	private void OnDrawGizmos() {

		Gizmos.color = Color.limeGreen;

		var previousMatrix = Gizmos.matrix;
		Gizmos.matrix = transform.localToWorldMatrix;

		Gizmos.DrawWireSphere(
			Vector3.down * _displayRadius,
			_displayRadius
		);

		Gizmos.matrix = previousMatrix;

	}

}