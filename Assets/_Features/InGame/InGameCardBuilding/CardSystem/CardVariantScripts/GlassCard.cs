public sealed class GlassCard : Card
{
    private void Awake() =>
        AddEffect(new CardEffect
        {
            DestroyOnEnemyHit = true
        });
}
