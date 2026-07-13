using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class CardHandDetector : MonoBehaviour
{
    [SerializeField]
    [Required]
    private CardField _cardField;

    [SerializeField]
    [Required]
    private CardHandRuleCatalog _ruleCatalog;

    private readonly List<CardHandRule> _catalogRules = new();
    private readonly List<CardHandRule> _runtimeRules = new();
    private readonly List<CardHandRule> _activeRules = new();
    private IReadOnlyList<CardHandMatch> _currentMatches = Array.Empty<CardHandMatch>();

    public event Action<IReadOnlyList<CardHandMatch>> MatchesChanged;

    public IReadOnlyList<CardHandMatch> CurrentMatches => _currentMatches;

    private void OnEnable()
    {
        ReloadCatalog(false);
        _cardField.CardsChanged += Recalculate;
        Recalculate();
    }

    private void OnDisable()
    {
        _cardField.CardsChanged -= Recalculate;
    }

    public void RegisterRule(CardHandRule rule)
    {
        EnsureUniqueRuleId(rule);
        _runtimeRules.Add(rule);
        RebuildActiveRules();

        if (isActiveAndEnabled) Recalculate();
    }

    public bool UnregisterRule(string ruleId)
    {
        for (int i = 0; i < _runtimeRules.Count; i++)
        {
            if (_runtimeRules[i].Id != ruleId) continue;

            _runtimeRules.RemoveAt(i);
            RebuildActiveRules();

            if (isActiveAndEnabled) Recalculate();

            return true;
        }

        return false;
    }

    public void ReloadCatalog()
    {
        ReloadCatalog(true);
    }

    public void Recalculate()
    {
        List<CardLine> lines = CardLineBuilder.Build(_cardField.PlacedCards);
        List<CardHandMatch> candidates = new();

        foreach (CardLine line in lines)
        {
            foreach (CardHandRule rule in _activeRules) rule.FindMatches(line, candidates);
        }

        _currentMatches = CardHandResultFilter.Filter(candidates);
        MatchesChanged?.Invoke(_currentMatches);
    }

    private void ReloadCatalog(bool recalculate)
    {
        _catalogRules.Clear();

        foreach (CardHandRule rule in _ruleCatalog.Rules)
        {
            EnsureUniqueRuleId(rule, _catalogRules);
            EnsureUniqueRuleId(rule, _runtimeRules);
            _catalogRules.Add(rule);
        }

        RebuildActiveRules();

        if (recalculate && isActiveAndEnabled) Recalculate();
    }

    private void RebuildActiveRules()
    {
        _activeRules.Clear();
        _activeRules.AddRange(_catalogRules);
        _activeRules.AddRange(_runtimeRules);
    }

    private void EnsureUniqueRuleId(CardHandRule rule)
    {
        EnsureUniqueRuleId(rule, _catalogRules);
        EnsureUniqueRuleId(rule, _runtimeRules);
    }

    private static void EnsureUniqueRuleId(
        CardHandRule rule,
        IReadOnlyList<CardHandRule> registeredRules
    )
    {
        foreach (CardHandRule registeredRule in registeredRules)
        {
            if (registeredRule.Id != rule.Id) continue;

            throw new InvalidOperationException($"Card hand rule ID is duplicated: {rule.Id}");
        }
    }
}
