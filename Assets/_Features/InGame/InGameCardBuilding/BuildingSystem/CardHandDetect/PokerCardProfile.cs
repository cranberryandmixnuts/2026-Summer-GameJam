using System;

public enum PokerHandParticipationMode
{
    Participant,
    Transparent,
    Blocker
}

public interface IPokerHandCard
{
    public PokerHandParticipationMode PokerHandParticipation { get; }
    public PokerCardProfile PokerProfile { get; }
}

public readonly struct PokerCardProfile
{
    public const int MinimumRank = 1;
    public const int MaximumRank = 13;
    public const ushort AllRankMask = (ushort)((1 << MaximumRank) - 1);
    public const byte AllPatternMask = (byte)((1 << 4) - 1);

    public ushort RankMask { get; }
    public byte PatternMask { get; }

    public bool IsValid => RankMask != 0 && PatternMask != 0;

    private PokerCardProfile(ushort rankMask, byte patternMask)
    {
        RankMask = rankMask;
        PatternMask = patternMask;
    }

    public static PokerCardProfile CreateStandard(int rank, CardPatternType pattern)
    {
        return new PokerCardProfile(GetRankMask(rank), GetPatternMask(pattern));
    }

    public static PokerCardProfile CreateRankWildcard(CardPatternType pattern)
    {
        return new PokerCardProfile(AllRankMask, GetPatternMask(pattern));
    }

    public static PokerCardProfile CreatePatternWildcard(int rank)
    {
        return new PokerCardProfile(GetRankMask(rank), AllPatternMask);
    }

    public static PokerCardProfile CreateWildcard()
    {
        return new PokerCardProfile(AllRankMask, AllPatternMask);
    }

    public static PokerCardProfile CreateFromMasks(ushort rankMask, byte patternMask)
    {
        if ((rankMask & AllRankMask) == 0) throw new ArgumentOutOfRangeException(nameof(rankMask));

        if ((patternMask & AllPatternMask) == 0) throw new ArgumentOutOfRangeException(nameof(patternMask));

        return new PokerCardProfile(
            (ushort)(rankMask & AllRankMask),
            (byte)(patternMask & AllPatternMask)
        );
    }

    public static ushort GetRankMask(int rank)
    {
        if (rank < MinimumRank || rank > MaximumRank) throw new ArgumentOutOfRangeException(nameof(rank));

        return (ushort)(1 << (rank - MinimumRank));
    }

    public static byte GetPatternMask(CardPatternType pattern)
    {
        int patternValue = (int)pattern;

        if (patternValue < (int)CardPatternType.Spade || patternValue > (int)CardPatternType.Clover) throw new ArgumentOutOfRangeException(nameof(pattern));

        return (byte)(1 << (patternValue - (int)CardPatternType.Spade));
    }
}
