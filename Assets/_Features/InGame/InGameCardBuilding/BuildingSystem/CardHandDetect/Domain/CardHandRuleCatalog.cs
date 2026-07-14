using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = nameof(CardHandRuleCatalog),
    menuName = "Cards/Hands/Card Hand Rule Catalog"
)]
public sealed class CardHandRuleCatalog : ScriptableObject
{
    [SerializeField]
    private List<CardHandRule> _rules = new();

    public IReadOnlyList<CardHandRule> Rules => _rules;

    public void ReplaceRules(IEnumerable<CardHandRule> rules)
    {
        _rules.Clear();
        _rules.AddRange(rules);
    }
}
