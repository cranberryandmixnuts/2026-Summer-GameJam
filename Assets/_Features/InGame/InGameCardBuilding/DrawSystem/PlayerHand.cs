using DG.Tweening;
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

	[Header("Animation")]
	[SerializeField] private float _cardAnimationDuration;
	[SerializeField] private Ease _cardAnimationEase;

	private readonly List<Card> _cards = new();
	private readonly Dictionary<Card, float> _cardPosition = new();

	//======================================================================| Properties

	public int HandCardLimit => _handCardLimit;
	public IReadOnlyList<Card> Cards => _cards;

	//======================================================================| Methods

	public void AddCard(Card card) {

		card.transform.SetParent(transform, false);

		_cards.Add(card);
		_cardPosition.Add(card, default);

		CalculateCardPosition();
		MoveCards();

	}

	private void CalculateCardPosition() {

		if (_cards.Count == 1) {
			_cardPosition[_cards[0]] = 0f;
			return;
		}
	
		var range = Mathf.Min(_displayMaxRange, _cards.Count * _displayNormalInterval);

		for (int i = 0; i < _cards.Count; i++) {

			var card = _cards[i];

			var factor = i * range / (_cards.Count - 1);
			_cardPosition[card] = factor - range / 2f;

		}

	}

	private void MoveCards() {
		
		foreach (var card in _cards) {

			card.transform.DOKill();

			card.transform
				.DOLocalMove(GetCardPosition(card), _cardAnimationDuration)
				.SetEase(_cardAnimationEase);

			card.transform
				.DOLocalRotate(Vector3.forward * GetCardAngle(card), _cardAnimationDuration)
				.SetEase(_cardAnimationEase);

		}

	}

	private Vector3 GetCardPosition(Card card) {
		
		var unitAngle = 1f / (2f * _displayRadius);
		var angle = -_cardPosition[card] * unitAngle + Mathf.PI / 2f;

		var position = _displayRadius * new Vector3(
			x: Mathf.Cos(angle),
			y: Mathf.Sin(angle)
		);

		return position + Vector3.down * _displayRadius;
	}

	private float GetCardAngle(Card card) {
			
		var unitAngle = 1f / (2f * _displayRadius);
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
		Gizmos.DrawWireSphere(transform.position - Vector3.up * _displayRadius, _displayRadius);

	}

}