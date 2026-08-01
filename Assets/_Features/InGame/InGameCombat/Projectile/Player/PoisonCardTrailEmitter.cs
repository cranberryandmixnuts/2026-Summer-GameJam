using UnityEngine;

[DisallowMultipleComponent]
public sealed class PoisonCardTrailEmitter : MonoBehaviour
{
    private Transform[] poisonCardTransforms;
    private PoisonAreaPool poisonAreaPool;
    private CardProjectileSettings settings;
    private LayerMask enemyLayers;
    private GameObject source;
    private float dropElapsedTime;

    public void Initialize(
        PoisonAreaPool poisonAreaPool,
        CardProjectileSettings settings,
        LayerMask enemyLayers,
        GameObject source
    )
    {
        PoisonCard[] poisonCards = GetComponentsInChildren<PoisonCard>(true);
        poisonCardTransforms = new Transform[poisonCards.Length];

        for (int index = 0; index < poisonCards.Length; index++)
            poisonCardTransforms[index] = poisonCards[index].transform;

        this.poisonAreaPool = poisonAreaPool;
        this.settings = settings;
        this.enemyLayers = enemyLayers;
        this.source = source;
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
                poisonAreaPool.Spawn(poisonCardTransform.position, settings, enemyLayers, source);
        }
    }
}
