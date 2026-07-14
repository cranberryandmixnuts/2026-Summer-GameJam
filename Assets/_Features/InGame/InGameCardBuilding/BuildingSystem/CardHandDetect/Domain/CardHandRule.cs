using System.Collections.Generic;
using UnityEngine;

public abstract class CardHandRule : ScriptableObject
{
    public abstract string Id { get; }

    public abstract void FindMatches(CardLine line, ICollection<CardHandMatch> matches);

    protected void AddMatch(
        string id,
        string displayName,
        int priority,
        Color lineColor,
        float bonus,
        CardLine line,
        IReadOnlyList<CardLineCard> cards,
        ICollection<CardHandMatch> matches
    )
    {
        matches.Add(new CardHandMatch(
            this,
            id,
            displayName,
            priority,
            lineColor,
            bonus,
            line,
            cards
        ));
    }
}
