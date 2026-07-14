using System.Collections.Generic;
using UnityEngine;

public sealed class CardHandMatch
{
    private readonly CardLineCard[] _cards;
    private readonly HashSet<Vector2Int> _positions;

    public CardHandRule Rule { get; }
    public string Id { get; }
    public string DisplayName { get; }
    public int Priority { get; }
    public Color LineColor { get; }
    public float Bonus { get; }
    public CardLine Line { get; }
    public IReadOnlyList<CardLineCard> Cards => _cards;

    public CardHandMatch(
        CardHandRule rule,
        string id,
        string displayName,
        int priority,
        Color lineColor,
        float bonus,
        CardLine line,
        IReadOnlyList<CardLineCard> cards
    )
    {
        Rule = rule;
        Id = id;
        DisplayName = displayName;
        Priority = priority;
        LineColor = lineColor;
        Bonus = bonus;
        Line = line;
        _cards = new CardLineCard[cards.Count];
        _positions = new HashSet<Vector2Int>();

        for (int i = 0; i < cards.Count; i++)
        {
            _cards[i] = cards[i];
            _positions.Add(cards[i].Position);
        }
    }

    public bool ContainsAll(CardHandMatch other)
    {
        foreach (CardLineCard card in other._cards)
        {
            if (!_positions.Contains(card.Position)) return false;
        }

        return true;
    }

    public bool HasSameIdentity(CardHandMatch other)
    {
        if (Id != other.Id) return false;
        if (_positions.Count != other._positions.Count) return false;

        return ContainsAll(other);
    }
}
