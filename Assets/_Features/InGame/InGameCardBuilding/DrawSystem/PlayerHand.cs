using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHand : SingletonBehaviour<PlayerHand, SceneScope>
{

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

    private readonly Dictionary<Card, Tween> _moveTweens = new();
    private readonly Dictionary<Card, Tween> _rotateTweens = new();
    private readonly Dictionary<Card, Tween> _scaleTweens = new();
    private readonly Dictionary<Card, RectTransform> _hoverRaycastAreas = new();
    private Coroutine _moveCardsCoroutine = null;

    //======================================================================| Properties

    public int HandCardLimit => _handCardLimit;
    public int RemainingCapacity => HandCardLimit - _cards.Count;
    public IReadOnlyList<Card> Cards => _cards;

    public int HoveredCardIndex { get; private set; } = -1;
    public Card HoveredCard { get; private set; } = null;

    public event Action<int, int> CapacityChanged;

    //======================================================================| Unity Methods

    private void Update()
    {

        HoveredCard = _cards.FirstOrDefault(card => card.IsHovered);

        HoveredCardIndex = HoveredCard != null
            ? _cards.IndexOf(HoveredCard)
            : -1;

        if (HoveredCard != _previousHoveredCard)
        {

            var skipSpreadingAnimation = IsHoverTransitionPlaying();
            var startCard = HoveredCard == null
                ? _previousHoveredCard
                : HoveredCard;

            var returningCard = _previousHoveredCard;
            var returnMoveDuration = returningCard == null
                ? 0f
                : _drawAnimationDuration * GetTweenProgress(_moveTweens, returningCard);
            var returnRotateDuration = returningCard == null
                ? 0f
                : _drawAnimationDuration * GetTweenProgress(_rotateTweens, returningCard);
            var returnScaleDuration = returningCard == null
                ? 0f
                : _drawAnimationDuration * GetTweenProgress(_scaleTweens, returningCard);

            if (returningCard != null)
                _hoverRaycastAreas[returningCard].gameObject.SetActive(false);

            if (HoveredCard != null)
                _hoverRaycastAreas[HoveredCard].gameObject.SetActive(true);

            CalculateCardPosition();

            if (returningCard != null)
                MoveCard(returningCard, returnMoveDuration, returnRotateDuration);

            MoveCards(
                _cards.IndexOf(startCard),
                skipSpreadingAnimation
                    ? 0f
                    : _hoverAnimationSpreadingDuration / _cards.Count,
                returningCard
            );

            if (HoveredCard != null)
            {
				HoveredCard.PlayHoverSound();
                SetTween(
                    _scaleTweens,
                    HoveredCard,
                    HoveredCard.transform
                        .DOScale(_hoverScale, _hoverAnimationDuration)
                        .SetEase(_hoverAnimationEase)
                );
            }

            if (returningCard != null)
            {
                SetTween(
                    _scaleTweens,
                    returningCard,
                    returningCard.transform
                        .DOScale(1f, returnScaleDuration)
                        .SetEase(_drawAnimationEase)
                );
            }

            _previousHoveredCard = HoveredCard;

        }

        if (HoveredCard != null)
            UpdateHoverRaycastArea(HoveredCard);

    }

    //======================================================================| Methods

    public void AddCard(Card card)
    {

        card.transform.SetParent(transform, false);

        if (card.PreviousIndex.HasValue)
        {
            _cards.Insert(card.PreviousIndex.Value, card);
        }
        else
        {
            _cards.Add(card);
        }

        _cardPosition.Add(card, default);
        _hoverRaycastAreas.Add(card, CreateHoverRaycastArea(card));

		CalculateCardPosition();
		MoveCards(_cards.Count - 1, 0f);
		card.PlayDrawSound();
		CapacityChanged?.Invoke(RemainingCapacity, HandCardLimit);

    }

    public void RemoveCard(Card card)
    {

        var removedIndex = _cards.IndexOf(card);
        card.PreviousIndex = removedIndex;

        if (_moveCardsCoroutine != null)
        {
            StopCoroutine(_moveCardsCoroutine);
            _moveCardsCoroutine = null;
        }

        KillTween(_moveTweens, card);
        KillTween(_rotateTweens, card);
        KillTween(_scaleTweens, card);

        card.transform.localScale = Vector3.one;

        _cards.Remove(card);
        _cardPosition.Remove(card);
        Destroy(_hoverRaycastAreas[card].gameObject);
        _hoverRaycastAreas.Remove(card);

        if (HoveredCard == card)
            HoveredCard = null;

        if (_previousHoveredCard == card)
            _previousHoveredCard = null;

        HoveredCardIndex = HoveredCard != null
            ? _cards.IndexOf(HoveredCard)
            : -1;

        CalculateCardPosition();

        if (_cards.Count > 0)
        {
            MoveCards(
                Mathf.Min(removedIndex, _cards.Count - 1),
                _hoverAnimationSpreadingDuration / _cards.Count
            );
        }

        CapacityChanged?.Invoke(RemainingCapacity, HandCardLimit);

    }

    private void CalculateCardPosition()
    {

        if (_cards.Count == 1)
        {
            _cardPosition[_cards[0]] = 0f;
            return;
        }

        var range = Mathf.Min(
            _displayMaxRange,
            (_cards.Count - 1) * _displayNormalInterval
        );

        var rawRange = range;
        if (HoveredCard != null) range += _hoverAdditionalPosition;

        for (int i = 0; i < _cards.Count; i++)
        {

            var card = _cards[i];
            var position = i * rawRange / (_cards.Count - 1);

            if (HoveredCard != null)
            {

                if (i > HoveredCardIndex)
                    position += _hoverAdditionalPosition;

                else if (i == HoveredCardIndex)
                    position += _hoverAdditionalPosition / 2f;

            }

            _cardPosition[card] = position - range / 2f;

        }

    }

    private void MoveCards(int startPosition, float timeInterval, Card excludedCard = null)
    {

        if (_moveCardsCoroutine != null)
            StopCoroutine(_moveCardsCoroutine);

        _moveCardsCoroutine = StartCoroutine(Routine());
        IEnumerator Routine()
        {

            int leftIndex = startPosition;
            int rightIndex = startPosition;

            while (leftIndex >= 0 || rightIndex < _cards.Count)
            {

                if (leftIndex >= 0 && _cards[leftIndex] != excludedCard)
                {
                    MoveCard(_cards[leftIndex]);
                }

                if (rightIndex < _cards.Count && rightIndex != leftIndex && _cards[rightIndex] != excludedCard)
                {
                    MoveCard(_cards[rightIndex]);
                }

                leftIndex--;
                rightIndex++;

                if (timeInterval != 0f)
                    yield return new WaitForSeconds(timeInterval);

            }

            _moveCardsCoroutine = null;

        }

    }

    private void MoveCard(Card card)
    {

        MoveCard(card, _drawAnimationDuration, _drawAnimationDuration);

    }

    private void MoveCard(Card card, float moveDuration, float rotateDuration)
    {

        SetTween(
            _moveTweens,
            card,
            card.transform
                .DOLocalMove(GetCardPosition(card), moveDuration)
                .SetEase(_drawAnimationEase)
        );

        SetTween(
            _rotateTweens,
            card,
            card.transform
                .DOLocalRotate(Vector3.forward * GetCardAngle(card), rotateDuration)
                .SetEase(_drawAnimationEase)
        );

    }

    private bool IsHoverTransitionPlaying() =>
        _moveCardsCoroutine != null
        || _moveTweens.Values.Any(tween => tween.IsActive() && tween.IsPlaying())
        || _rotateTweens.Values.Any(tween => tween.IsActive() && tween.IsPlaying())
        || _scaleTweens.Values.Any(tween => tween.IsActive() && tween.IsPlaying());

    private float GetTweenProgress(Dictionary<Card, Tween> tweens, Card card)
    {

        if (!tweens.TryGetValue(card, out var tween) || !tween.IsActive()) return 1f;

        return tween.ElapsedPercentage(false);

    }

    private void SetTween(Dictionary<Card, Tween> tweens, Card card, Tween tween)
    {

        KillTween(tweens, card);
        tweens[card] = tween;

        tween.OnKill(() => {
            if (tweens.TryGetValue(card, out var currentTween) && currentTween == tween)
                tweens.Remove(card);
        });

    }

    private void KillTween(Dictionary<Card, Tween> tweens, Card card)
    {

        if (tweens.TryGetValue(card, out var tween))
            tween.Kill();

    }

    private RectTransform CreateHoverRaycastArea(Card card)
    {

        var raycastAreaObject = new GameObject(
            "Hover Raycast Area",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        var raycastArea = (RectTransform)raycastAreaObject.transform;
        raycastArea.SetParent(card.transform, false);
        raycastArea.SetAsFirstSibling();
        raycastArea.anchorMin = new Vector2(0f, 0f);
        raycastArea.anchorMax = new Vector2(1f, 0f);
        raycastArea.pivot = new Vector2(0.5f, 1f);
        raycastArea.anchoredPosition = Vector2.zero;
        raycastArea.sizeDelta = Vector2.zero;

        var image = raycastAreaObject.GetComponent<Image>();
        image.color = Color.clear;
        image.raycastTarget = true;
        image.maskable = false;

        raycastAreaObject.SetActive(false);

        return raycastArea;
    }

    private void UpdateHoverRaycastArea(Card card)
    {

        var cardRectTransform = (RectTransform)card.transform;
        var canvas = GetComponentInParent<Canvas>().rootCanvas;
        var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        var screenPosition = RectTransformUtility.WorldToScreenPoint(
            camera,
            cardRectTransform.TransformPoint(cardRectTransform.rect.center)
        );
        screenPosition.y = Screen.safeArea.yMin;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            cardRectTransform,
            screenPosition,
            camera,
            out var screenBottomPosition
        );

        _hoverRaycastAreas[card].sizeDelta = new Vector2(
            0f,
            Mathf.Max(0f, cardRectTransform.rect.yMin - screenBottomPosition.y)
        );
    }

    private Vector3 GetCardPosition(Card card)
    {

        var unitAngle = 1f / _displayRadius;
        var angle = -_cardPosition[card] * unitAngle + Mathf.PI / 2f;

        var position = _displayRadius * new Vector3(
            x: Mathf.Cos(angle),
            y: Mathf.Sin(angle)
        ) + Vector3.down * _displayRadius;

        if (card.IsHovered)
            position.y = GetHoverPositionY();

        return position;
    }

    private float GetHoverPositionY()
    {

        var rectTransform = (RectTransform)transform;
        var canvas = GetComponentInParent<Canvas>().rootCanvas;
        var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            new Vector2(Screen.safeArea.center.x, Screen.safeArea.yMin),
            camera,
            out var screenBottomPosition
        );

        return screenBottomPosition.y + _hoverHeight;
    }

    private float GetCardAngle(Card card)
    {

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

    private void OnDrawGizmos()
    {

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
