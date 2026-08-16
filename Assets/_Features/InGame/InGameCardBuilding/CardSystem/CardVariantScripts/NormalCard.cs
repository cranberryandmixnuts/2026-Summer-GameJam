using UnityEngine;

public class NormalCard : Card, IPokerHandCard
{
    [SerializeField]
    private CardPatternType _pattern;

    [SerializeField]
    [Range(PokerCardProfile.MinimumRank, PokerCardProfile.MaximumRank)]
    private int _number = PokerCardProfile.MinimumRank;

    public CardPatternType Pattern => _pattern;
    public int Number => _number;
    public PokerHandParticipationMode PokerHandParticipation => PokerHandParticipationMode.Participant;
    public PokerCardProfile PokerProfile => PokerCardProfile.CreateStandard(_number, _pattern);
}
