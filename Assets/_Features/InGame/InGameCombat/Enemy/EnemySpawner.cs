using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemySpawner : MonoBehaviour
{
    [Serializable]
    private sealed class SpawnEntry
    {
        [Required] public GameObject Prefab;
        [MinValue(0.01f)] public float Weight = 1f;
    }

    [SerializeField, Required] private BoxCollider2D spawnArea;
    [SerializeField, Required] private Transform enemiesRoot;
    [SerializeField, Required] private Transform player;
    [SerializeField, Required] private Collider2D playerCollider;
    [SerializeField, Required] private PlayerHealth playerHealth;
    [SerializeField, Required] private EnemyProjectilePool projectilePool;
    [SerializeField, Required] private DespawnBounds despawnBounds;
    [SerializeField, ValidateInput(nameof(HasSpawnEntries), "적 프리팹을 하나 이상 등록해야 합니다.")]
    private SpawnEntry[] spawnEntries;
    [SerializeField, MinValue(0f)] private float initialDelay = 1f;
    [SerializeField, MinValue(0.01f)] private float spawnInterval = 1.5f;
    [SerializeField] private bool spawnOnEnable = true;

    public bool IsSpawning => spawnRoutine != null;

    private readonly List<MonoBehaviour> initializationBuffer = new();

    private Coroutine spawnRoutine;
    private EnemyRuntimeContext runtimeContext;
    private float totalWeight;

    private void Awake()
    {
        runtimeContext = new EnemyRuntimeContext(
            player,
            playerCollider,
            playerHealth,
            projectilePool,
            despawnBounds);

        for (int i = 0; i < spawnEntries.Length; i++) totalWeight += spawnEntries[i].Weight;
    }

    private void OnEnable()
    {
        playerHealth.Died += HandlePlayerDied;

        if (spawnOnEnable && runtimeContext.IsCombatActive) StartSpawning();
    }

    private void OnDisable()
    {
        playerHealth.Died -= HandlePlayerDied;
        StopSpawning();
    }

    public void StartSpawning()
    {
        if (spawnRoutine != null || !runtimeContext.IsCombatActive) return;

        spawnRoutine = StartCoroutine(SpawnSequence());
    }

    public void StopSpawning()
    {
        if (spawnRoutine == null) return;

        StopCoroutine(spawnRoutine);
        spawnRoutine = null;
    }

    public GameObject SpawnImmediately()
    {
        if (!runtimeContext.IsCombatActive) return null;

        SpawnEntry entry = SelectSpawnEntry();
        Bounds bounds = spawnArea.bounds;
        Vector2 position = new(
            UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
            UnityEngine.Random.Range(bounds.min.y, bounds.max.y));

        GameObject enemy = Instantiate(entry.Prefab, position, Quaternion.identity, enemiesRoot);
        InitializeEnemy(enemy);
        return enemy;
    }

    private IEnumerator SpawnSequence()
    {
        if (initialDelay > 0f) yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            SpawnImmediately();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private SpawnEntry SelectSpawnEntry()
    {
        float selection = UnityEngine.Random.value * totalWeight;

        for (int i = 0; i < spawnEntries.Length; i++)
        {
            selection -= spawnEntries[i].Weight;
            if (selection <= 0f) return spawnEntries[i];
        }

        return spawnEntries[spawnEntries.Length - 1];
    }

    private void InitializeEnemy(GameObject enemy)
    {
        initializationBuffer.Clear();
        enemy.GetComponentsInChildren(true, initializationBuffer);

        for (int i = 0; i < initializationBuffer.Count; i++) InitializeComponent(initializationBuffer[i]);
    }

    private void InitializeComponent(MonoBehaviour component)
    {
        if (component is IEnemyRuntimeInitializable initializable) initializable.Initialize(runtimeContext);
    }

    private void HandlePlayerDied() => StopSpawning();

    private bool HasSpawnEntries(SpawnEntry[] value) => value != null && value.Length > 0;
}
