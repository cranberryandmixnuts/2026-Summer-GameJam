using System;
using System.Collections.Generic;

public static class CardHandResultFilter
{
    public static List<CardHandMatch> Filter(IReadOnlyList<CardHandMatch> candidates)
    {
        List<CardHandMatch> uniqueMatches = RemoveDuplicates(candidates);
        List<CardHandMatch> filteredMatches = new();

        foreach (CardHandMatch candidate in uniqueMatches)
        {
            if (IsContainedByDominantMatch(candidate, uniqueMatches)) continue;

            filteredMatches.Add(candidate);
        }

        filteredMatches.Sort(CompareMatches);
        return filteredMatches;
    }

    private static List<CardHandMatch> RemoveDuplicates(IReadOnlyList<CardHandMatch> candidates)
    {
        List<CardHandMatch> uniqueMatches = new();

        foreach (CardHandMatch candidate in candidates)
        {
            bool isDuplicate = false;

            foreach (CardHandMatch uniqueMatch in uniqueMatches)
            {
                if (!candidate.HasSameIdentity(uniqueMatch)) continue;

                isDuplicate = true;
                break;
            }

            if (!isDuplicate) uniqueMatches.Add(candidate);
        }

        return uniqueMatches;
    }

    private static bool IsContainedByDominantMatch(
        CardHandMatch candidate,
        IReadOnlyList<CardHandMatch> matches
    )
    {
        foreach (CardHandMatch other in matches)
        {
            if (ReferenceEquals(candidate, other)) continue;
            if (other.Cards.Count < candidate.Cards.Count) continue;
            if (!other.ContainsAll(candidate)) continue;
            if (other.Cards.Count > candidate.Cards.Count) return true;
            if (other.Priority > candidate.Priority) return true;
        }

        return false;
    }

    private static int CompareMatches(CardHandMatch left, CardHandMatch right)
    {
        int priorityComparison = right.Priority.CompareTo(left.Priority);
        if (priorityComparison != 0) return priorityComparison;

        int directionComparison = left.Line.Direction.CompareTo(right.Line.Direction);
        if (directionComparison != 0) return directionComparison;

        int positionComparison = ComparePositions(left.Cards[0].Position, right.Cards[0].Position);
        if (positionComparison != 0) return positionComparison;

        return string.Compare(left.Id, right.Id, StringComparison.Ordinal);
    }

    private static int ComparePositions(UnityEngine.Vector2Int left, UnityEngine.Vector2Int right)
    {
        int yComparison = left.y.CompareTo(right.y);
        return yComparison != 0 ? yComparison : left.x.CompareTo(right.x);
    }
}
