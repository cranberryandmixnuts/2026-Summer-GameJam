using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(
    fileName = nameof(CardDatabase),
    menuName = "Cards/" + nameof(CardDatabase)
)]
public class CardDatabase : ScriptableObject
{
    [SerializeField] private List<Card> _cards;

    private List<Card> _normalCards;
    private List<Card> _specialCards;

    public IReadOnlyList<Card> Cards => _cards;
    public IReadOnlyList<Card> NormalCards => _normalCards;
    public IReadOnlyList<Card> SpecialCards => _specialCards;

    public void Initialize()
    {
        _normalCards = _cards.Where(card => !card.IsSpecial).ToList();
        _specialCards = _cards.Where(card => card.IsSpecial).ToList();
    }
}
