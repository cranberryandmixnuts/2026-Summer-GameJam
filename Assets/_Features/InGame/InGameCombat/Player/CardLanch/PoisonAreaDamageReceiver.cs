using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public sealed class PoisonAreaDamageReceiver : BaseBehaviour
{
    private sealed class Contribution
    {
        public PoisonArea Area { get; }
        public int Damage { get; }
        public float DamageInterval { get; }
        public GameObject Source { get; }

        public Contribution(
            PoisonArea area,
            int damage,
            float damageInterval,
            GameObject source
        )
        {
            Area = area;
            Damage = damage;
            DamageInterval = damageInterval;
            Source = source;
        }
    }

    private readonly List<Contribution> contributions = new();

    private EnemyHealth health;
    private float damageTimeRemaining;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        enabled = false;
    }

    public void Register(
        PoisonArea area,
        int damage,
        float damageInterval,
        GameObject source
    )
    {
        if (FindContribution(area) != null) return;

        Contribution contribution = new(area, damage, damageInterval, source);
        bool wasActive = contributions.Count > 0;
        contributions.Add(contribution);

        if (wasActive) return;

        DealDamage(contribution);
        damageTimeRemaining = damageInterval;
        enabled = !health.IsDead;
    }

    public void Unregister(PoisonArea area)
    {
        Contribution contribution = FindContribution(area);
        if (contribution == null) return;

        contributions.Remove(contribution);
        if (contributions.Count > 0) return;

        damageTimeRemaining = 0f;
        enabled = false;
    }

    private void Update()
    {
        Contribution contribution = GetStrongestContribution();
        damageTimeRemaining -= Time.deltaTime;

        while (damageTimeRemaining <= 0f && !health.IsDead)
        {
            DealDamage(contribution);
            damageTimeRemaining += contribution.DamageInterval;
        }

        if (!health.IsDead) return;

        contributions.Clear();
        enabled = false;
    }

    private void DealDamage(Contribution contribution)
    {
        DamageInfo damageInfo = new(
            contribution.Damage,
            contribution.Source,
            health.transform.position,
            Vector2.zero);

        health.TryTakeDamage(damageInfo);
    }

    private Contribution FindContribution(PoisonArea area)
    {
        foreach (Contribution contribution in contributions)
        {
            if (contribution.Area == area) return contribution;
        }

        return null;
    }

    private Contribution GetStrongestContribution()
    {
        Contribution strongestContribution = contributions[0];

        for (int index = 1; index < contributions.Count; index++)
        {
            Contribution contribution = contributions[index];

            if (contribution.Damage > strongestContribution.Damage) strongestContribution = contribution;
        }

        return strongestContribution;
    }
}
