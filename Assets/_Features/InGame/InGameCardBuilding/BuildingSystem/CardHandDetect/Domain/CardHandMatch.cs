using System.Collections.Generic;
using UnityEngine;

public sealed class CardHandMatch
{
    private readonly CardLineCard[] _cards;
    private readonly HashSet<Vector2Int> _positions;

    public CardHandRule Rule { get; }
    public CardLine Line { get; }
    public IReadOnlyList<CardLineCard> Cards => _cards;

    public CardHandMatch(CardHandRule rule, CardLine line, IReadOnlyList<CardLineCard> cards)
    {
        Rule = rule;
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
        if (Rule.Id != other.Rule.Id) return false;
        if (_positions.Count != other._positions.Count) return false;

        return ContainsAll(other);
    }
}
