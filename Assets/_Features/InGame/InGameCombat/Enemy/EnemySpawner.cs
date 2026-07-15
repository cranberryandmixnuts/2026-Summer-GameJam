using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField, Required] private CombatBridge combatBridge;
    [SerializeField, Required] private CombatBounds combatBounds;
    [SerializeField, Required] private DespawnBounds despawnBounds;
    [SerializeField, Required] private Image timerFillImage;
    [SerializeField, Required] private RectTransform healthBarsRoot;
    [SerializeField, Required] private EnemyHealthBar healthBarPrefab;
    [SerializeField, Required] private Camera worldCamera;
    [SerializeField] private Vector3 healthBarOffset = new(0f, 1f, 0f);
    [SerializeField, Required] private GameObject bossPrefab;
    [SerializeField, ValidateInput(nameof(HasSpawnEntries), "적 프리팹을 하나 이상 등록해야 합니다.")]
    private SpawnEntry[] spawnEntries;
    [SerializeField, MinValue(0f)] private float initialDelay = 1f;
    [SerializeField, MinValue(0.01f)] private float spawnDuration = 30f;
    [SerializeField, MinValue(0.01f)] private float minimumSpawnInterval = 1f;
    [SerializeField, MinValue(0.01f), ValidateInput(nameof(IsValidMaximumSpawnInterval), "최대 스폰 간격은 최소 스폰 간격 이상이어야 합니다.")]
    private float maximumSpawnInterval = 2f;
    [SerializeField] private bool spawnOnEnable = true;

    public bool IsSpawning => spawnRoutine != null;

    private readonly List<MonoBehaviour> initializationBuffer = new();

    private Coroutine spawnRoutine;
    private EnemyRuntimeContext runtimeContext;
    private float totalWeight;
    private bool hasPlayerDied;

    private void Awake()
    {
        runtimeContext = new EnemyRuntimeContext(
            player,
            playerCollider,
            playerHealth,
            combatBridge,
            combatBounds,
            despawnBounds);

        for (int i = 0; i < spawnEntries.Length; i++) totalWeight += spawnEntries[i].Weight;

        timerFillImage.fillAmount = 0f;
        combatBridge.PlayerDied += HandlePlayerDied;
    }

    private void OnEnable()
    {
        if (spawnOnEnable && !hasPlayerDied) StartSpawning();
    }

    private void OnDisable() => StopSpawning();

    private void OnDestroy() => combatBridge.PlayerDied -= HandlePlayerDied;

    public void StartSpawning()
    {
        if (spawnRoutine != null || hasPlayerDied) return;

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
        if (hasPlayerDied) return null;

        SpawnEntry entry = SelectSpawnEntry();
        Bounds bounds = spawnArea.bounds;
        Vector2 position = new(
            UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
            UnityEngine.Random.Range(bounds.min.y, bounds.max.y));

        return SpawnPrefab(entry.Prefab, position);
    }

    private IEnumerator SpawnSequence()
    {
        timerFillImage.fillAmount = 0f;

        if (initialDelay > 0f) yield return new WaitForSeconds(initialDelay);

        float elapsedTime = 0f;
        float nextSpawnTime = 0f;

        while (elapsedTime < spawnDuration)
        {
            if (elapsedTime >= nextSpawnTime)
            {
                SpawnImmediately();
                nextSpawnTime = elapsedTime + UnityEngine.Random.Range(
                    minimumSpawnInterval,
                    maximumSpawnInterval);
            }

            elapsedTime += Time.deltaTime;
            timerFillImage.fillAmount = Mathf.Clamp01(elapsedTime / spawnDuration);
            yield return null;
        }

        timerFillImage.fillAmount = 1f;
        SpawnCompletionPrefab();
        spawnRoutine = null;
    }

    private GameObject SpawnCompletionPrefab()
    {
        Vector2 position = spawnArea.bounds.center;
        return SpawnPrefab(bossPrefab, position);
    }

    private GameObject SpawnPrefab(GameObject prefab, Vector2 position)
    {
        GameObject enemy = Instantiate(
            prefab,
            position,
            Quaternion.identity,
            enemiesRoot);

        InitializeEnemy(enemy);
        AttachHealthBar(enemy);
        return enemy;
    }

    private SpawnEntry SelectSpawnEntry()
    {
        float selection = UnityEngine.Random.value * totalWeight;

        for (int i = 0; i < spawnEntries.Length; i++)
        {
            selection -= spawnEntries[i].Weight;
            if (selection <= 0f) return spawnEntries[i];
        }

        return spawnEntries[^1];
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

    private void AttachHealthBar(GameObject enemy)
    {
        EnemyHealth enemyHealth = enemy.GetComponentInChildren<EnemyHealth>(true);

        if (enemyHealth == null)
            throw new MissingComponentException($"{enemy.name}에 {nameof(EnemyHealth)}가 없습니다.");

        EnemyHealthBar healthBar = Instantiate(healthBarPrefab, healthBarsRoot);
        healthBar.Initialize(enemyHealth, healthBarsRoot, worldCamera, healthBarOffset);
    }

    private void HandlePlayerDied()
    {
        hasPlayerDied = true;
        StopSpawning();
    }

    private bool HasSpawnEntries(SpawnEntry[] value) => value != null && value.Length > 0;

    private bool IsValidMaximumSpawnInterval(float value) => value >= minimumSpawnInterval;
}