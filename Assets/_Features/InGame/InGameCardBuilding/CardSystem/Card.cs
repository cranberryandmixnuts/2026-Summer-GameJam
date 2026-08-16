using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public abstract class Card : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    private const float SlotReactiveRange = 7.5f;

    [Header("Status")]
    [SerializeField] private float _baseDamage;
    [SerializeField] private float _additionalMultiplier;
    [SerializeField, TextArea] private string _explanation;
    [SerializeField] private bool _isJoker;
    [SerializeField] private bool _isSpecial;
    [SerializeField] private bool _isBlockingAttachment;

    private GameObject _currentTargetSlot;
    protected readonly Dictionary<int, Card> _cardOnSpecialSlot = new();

    public float BaseDamage => _baseDamage;
    public float AdditionalMultiplier => _additionalMultiplier;
    public string Explanation => _explanation;
    public bool IsJoker => _isJoker;
    public bool IsSpecial => _isSpecial;
    public bool IsBlockingAttachment => _isBlockingAttachment;
    public bool IsHovered { get; private set; }
    public bool IsGrabed { get; private set; }
    public bool IsAttached { get; private set; }
    public int? PreviousIndex { get; set; }
    public GameObject AttachedSlot { get; private set; }
    public CardEffect Effect { get; private set; } = new();
    public virtual IReadOnlyList<SpecialCardSlot> SpecialCardSlots => Array.Empty<SpecialCardSlot>();

    public event Action<Card> OnUpdate;

    protected virtual void Update()
    {
        if (AttachedSlot == null)
        {
            if (IsGrabed)
            {
                transform.position = Camera.main
                    .ScreenToWorldPoint(Mouse.current.position.ReadValue().ToVector3WithZ(100f))
                    .WithZ(transform.position.z);

                _currentTargetSlot = GetAttachSlot();
            }

            OnUpdate?.Invoke(this);
        }

        foreach ((int index, Card card) in _cardOnSpecialSlot)
        {
            card.transform.SetPositionAndRotation(
                SpecialCardSlots[index].transform.position,
                SpecialCardSlots[index].transform.rotation
            );
        }
    }

    public virtual float CalculateDamage() => BaseDamage;
    public virtual float CalculateAdditionalMultiplier() => AdditionalMultiplier;

    public void PlayDrawSound() => AudioManager.Instance.PlayOneShotSFX("CardDraw", gameObject);
    public void PlayHoverSound() => AudioManager.Instance.PlayOneShotSFX("CardHovered", gameObject);
    public void PlayPlaceSound() => AudioManager.Instance.PlayOneShotSFX("CardPlace", gameObject);

    public void AddCardOnSpecialSlot(Card target, int index) => _cardOnSpecialSlot.Add(index, target);

    public void AddCardOnSpecialSlot(Card target, SpecialCardSlot slot) =>
        _cardOnSpecialSlot.Add(SpecialCardSlots.IndexOf(slot), target);

    public void AddEffect(CardEffect effect) => Effect += effect;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsAttached) return;

        IsHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsGrabed) IsHovered = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsAttached) return;

        IsGrabed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (AttachedSlot != null) return;

        IsGrabed = false;
        IsHovered = false;

        if (_currentTargetSlot == null) return;

        IsAttached = true;
        AttachedSlot = _currentTargetSlot;

        PlayerHand.Instance.RemoveCard(this);
        CardField.Instance.PlaceCard(AttachedSlot, this);
    }

    private GameObject GetAttachSlot()
    {
        IEnumerable<GameObject> slots = CardField.Instance.SlotInstances.Keys
            .Concat(CardField.Instance.SpecialSlotInstances
                .Where(slot => slot.PlacedCard == null)
                .Select(slot => slot.gameObject)
            )
            .Where(slot => Vector2.Distance(slot.transform.position, transform.position) <= SlotReactiveRange);

        if (!slots.Any()) return null;

        float minimumDistance = float.MaxValue;
        GameObject result = null;

        foreach (GameObject slot in slots)
        {
            float distance = Vector2.Distance(slot.transform.position, transform.position);

            if (distance >= minimumDistance) continue;

            minimumDistance = distance;
            result = slot;
        }

        return result;
    }
}
