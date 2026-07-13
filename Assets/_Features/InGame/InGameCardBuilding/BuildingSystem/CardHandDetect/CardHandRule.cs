using System.Collections.Generic;
using UnityEngine;

public abstract class CardHandRule : ScriptableObject
{
    [SerializeField]
    private List<CardHandBonus> _bonuses = new();

    public abstract string Id { get; }
    public abstract string DisplayName { get; }
    public abstract int Priority { get; }
    public abstract Color LineColor { get; }
    public IReadOnlyList<CardHandBonus> Bonuses => _bonuses;

    public abstract void FindMatches(CardLine line, ICollection<CardHandMatch> matches);

    protected void AddMatch(
        CardLine line,
        IReadOnlyList<CardLineCard> cards,
        ICollection<CardHandMatch> matches
    )
    {
        matches.Add(new CardHandMatch(this, line, cards));
    }
}
