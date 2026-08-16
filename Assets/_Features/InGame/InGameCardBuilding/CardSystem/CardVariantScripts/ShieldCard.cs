public sealed class ShieldCard : Card
{
    private void Awake() =>
        AddEffect(new CardEffect
        {
            DestroysEnemyProjectiles = true
        });
}
