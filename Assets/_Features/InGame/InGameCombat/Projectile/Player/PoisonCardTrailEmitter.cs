using UnityEngine;

public sealed class PoisonCardTrailEmitter : MonoBehaviour
{
    private Transform[] poisonCardTransforms;
    private PoisonAreaSpawner poisonAreaSpawner;
    private CardProjectileSettings settings;
    private LayerMask enemyLayers;
    private GameObject source;
    private int finalProjectileDamage;
    private float dropElapsedTime;

    public void Initialize(
        PoisonAreaSpawner poisonAreaSpawner,
        CardProjectileSettings settings,
        LayerMask enemyLayers,
        GameObject source,
        int finalProjectileDamage
    )
    {
        PoisonCard[] poisonCards = GetComponentsInChildren<PoisonCard>(true);
        poisonCardTransforms = new Transform[poisonCards.Length];

        for (int index = 0; index < poisonCards.Length; index++) poisonCardTransforms[index] = poisonCards[index].transform;

        this.poisonAreaSpawner = poisonAreaSpawner;
        this.settings = settings;
        this.enemyLayers = enemyLayers;
        this.source = source;
        this.finalProjectileDamage = finalProjectileDamage;
        dropElapsedTime = 0f;
        enabled = poisonCardTransforms.Length > 0;
    }

    private void Update()
    {
        dropElapsedTime += Time.deltaTime;

        while (dropElapsedTime >= settings.PoisonDropInterval)
        {
            dropElapsedTime -= settings.PoisonDropInterval;

            foreach (Transform poisonCardTransform in poisonCardTransforms)
            {
                poisonAreaSpawner.Spawn(
                    poisonCardTransform.position,
                    settings,
                    enemyLayers,
                    source,
                    finalProjectileDamage
                );
            }
        }
    }
}
