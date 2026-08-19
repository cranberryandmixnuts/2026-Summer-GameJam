using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public sealed class GameManager : MonoBehaviour
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
    [SerializeField, Required] private RectTransform healthBarsRoot;
    [SerializeField, Required] private EnemyHealthBar healthBarPrefab;
    [SerializeField, Required] private Camera worldCamera;
    [SerializeField] private Vector3 healthBarOffset = new(0f, 1f, 0f);

    [TitleGroup("UI")]
    [SerializeField, Required, LabelText("경과 시간 텍스트")]
    private TMP_Text elapsedTimeText;

    [TitleGroup("엔딩")]
    [SerializeField, Required, LabelText("실패 화면")]
    private GameObject failureEndingObject;

    [TitleGroup("엔딩")]
    [SerializeField, Required, LabelText("설정창 열기 오브젝트")]
    private GameObject settingsOpenObject;

    [SerializeField, ValidateInput(nameof(HasSpawnEntries), "적 프리팹을 하나 이상 등록해야 합니다.")]
    private SpawnEntry[] spawnEntries;

    [SerializeField, MinValue(0f)] private float initialDelay = 1f;
    [SerializeField, MinValue(0.01f)] private float minimumSpawnInterval = 1f;

    [SerializeField, MinValue(0.01f), ValidateInput(nameof(IsValidMaximumSpawnInterval), "최대 스폰 간격은 최소 스폰 간격 이상이어야 합니다.")]
    private float maximumSpawnInterval = 2f;

    [TitleGroup("난이도")]
    [SerializeField, MinValue(EnemyDifficultyUtility.MinimumDifficultyFactor), LabelText("최소 내부 난이도 인수")]
    private float minimumInternalDifficultyFactor = 1f;

    [TitleGroup("난이도")]
    [SerializeField, MinValue(0f), LabelText("초당 내부 난이도 인수 증가량")]
    private float internalDifficultyIncreasePerSecond = 0.01f;

    [SerializeField] private bool spawnOnEnable = true;

    public bool IsSpawning => spawnRoutine != null;

    [TitleGroup("난이도"), ShowInInspector, ReadOnly, LabelText("현재 내부 난이도 인수")]
    public float InternalDifficultyFactor { get; private set; } = 1f;

    public float FinalDifficultyFactor =>
        InternalDifficultyFactor * combatBridge.ExternalDifficultyFactor;

    private readonly List<MonoBehaviour> initializationBuffer = new();

    private Coroutine spawnRoutine;
    private EnemyRuntimeContext runtimeContext;
    private RunStatisticsRepository runStatisticsRepository;
    private float totalWeight;
    private float elapsedTime;
    private int displayedElapsedSeconds = -1;
    private int firedProjectileCount;
    private bool hasGameEnded;
    private bool hasRunStarted;
    private bool hasRunBeenRecorded;

    private void Awake()
    {
        runStatisticsRepository = new RunStatisticsRepository();
        runtimeContext = new EnemyRuntimeContext(
            player,
            playerCollider,
            playerHealth,
            combatBridge,
            combatBounds,
            despawnBounds);

        for (int i = 0; i < spawnEntries.Length; i++) totalWeight += spawnEntries[i].Weight;

        InternalDifficultyFactor = minimumInternalDifficultyFactor;
        UpdateElapsedTimeText(0f);
        failureEndingObject.SetActive(false);
        combatBridge.PlayerDied += HandlePlayerDied;
        combatBridge.ProjectileFired += HandleProjectileFired;
    }

    private void OnEnable()
    {
        if (spawnOnEnable && !hasGameEnded) StartSpawning();
    }

    private void OnDisable() => StopSpawning();

    private void OnDestroy()
    {
        RecordRun();
        combatBridge.PlayerDied -= HandlePlayerDied;
        combatBridge.ProjectileFired -= HandleProjectileFired;
    }

    public void StartSpawning()
    {
        if (spawnRoutine != null || hasGameEnded) return;

        if (!hasRunStarted) BeginRun();

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
        if (hasGameEnded) return null;

        SpawnEntry entry = SelectSpawnEntry();
        Bounds bounds = spawnArea.bounds;
        Vector2 position = new(
            UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
            UnityEngine.Random.Range(bounds.min.y, bounds.max.y));

        return SpawnPrefab(entry.Prefab, position);
    }

    private IEnumerator SpawnSequence()
    {
        if (initialDelay > 0f) yield return new WaitForSeconds(initialDelay);

        float nextSpawnTime = elapsedTime;

        while (!hasGameEnded)
        {
            InternalDifficultyFactor =
                minimumInternalDifficultyFactor +
                elapsedTime * internalDifficultyIncreasePerSecond;

            if (elapsedTime >= nextSpawnTime)
            {
                SpawnImmediately();
                nextSpawnTime = elapsedTime + GetRandomSpawnInterval();
            }

            elapsedTime += Time.deltaTime;
            UpdateElapsedTimeText(elapsedTime);
            yield return null;
        }

        spawnRoutine = null;
    }

    private GameObject SpawnPrefab(GameObject prefab, Vector2 position)
    {
        GameObject enemy = Instantiate(
            prefab,
            position,
            Quaternion.identity,
            enemiesRoot);

        InitializeEnemy(enemy);

        EnemyHealth enemyHealth = GetEnemyHealth(enemy);
        AttachHealthBar(enemyHealth);
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

        float difficultyFactor = FinalDifficultyFactor;

        for (int i = 0; i < initializationBuffer.Count; i++)
        {
            if (initializationBuffer[i] is IEnemyDifficultyInitializable difficultyInitializable)
                difficultyInitializable.InitializeDifficulty(difficultyFactor);
        }

        for (int i = 0; i < initializationBuffer.Count; i++)
            InitializeRuntimeComponent(initializationBuffer[i]);
    }

    private void InitializeRuntimeComponent(MonoBehaviour component)
    {
        if (component is IEnemyRuntimeInitializable initializable)
            initializable.Initialize(runtimeContext);
    }

    private EnemyHealth GetEnemyHealth(GameObject enemy)
    {
        EnemyHealth enemyHealth = enemy.GetComponentInChildren<EnemyHealth>(true);

        if (enemyHealth == null)
            throw new MissingComponentException(
                $"{enemy.name}에 {nameof(EnemyHealth)}가 없습니다.");

        return enemyHealth;
    }

    private void AttachHealthBar(EnemyHealth enemyHealth)
    {
        EnemyHealthBar healthBar = Instantiate(healthBarPrefab, healthBarsRoot);
        healthBar.Initialize(
            enemyHealth,
            healthBarsRoot,
            worldCamera,
            healthBarOffset);
    }

    private float GetRandomSpawnInterval() =>
        UnityEngine.Random.Range(minimumSpawnInterval, maximumSpawnInterval) /
        EnemyDifficultyUtility.ClampFactor(FinalDifficultyFactor);

    private void UpdateElapsedTimeText(float elapsedTime)
    {
        int totalSeconds = Mathf.FloorToInt(elapsedTime);
        if (totalSeconds == displayedElapsedSeconds) return;

        displayedElapsedSeconds = totalSeconds;

        int hours = totalSeconds / 3600;
        int minutes = totalSeconds / 60 % 60;
        int seconds = totalSeconds % 60;
        elapsedTimeText.text = $"{hours:00}:{minutes:00}:{seconds:00}";
    }

    private void HandlePlayerDied() => EndGame();

    private void HandleProjectileFired()
    {
        if (!hasRunStarted || hasGameEnded) return;

        firedProjectileCount++;
    }

    private void BeginRun()
    {
        hasRunStarted = true;
        elapsedTime = 0f;
        firedProjectileCount = 0;
        InternalDifficultyFactor = minimumInternalDifficultyFactor;
        UpdateElapsedTimeText(elapsedTime);
    }

    private void RecordRun()
    {
        if (!hasRunStarted || hasRunBeenRecorded) return;

        runStatisticsRepository.AddRun(elapsedTime, firedProjectileCount);
        hasRunBeenRecorded = true;
    }

    private void EndGame()
    {
        if (hasGameEnded) return;

        hasGameEnded = true;
        StopSpawning();
        settingsOpenObject.SetActive(false);
        RecordRun();
        failureEndingObject.SetActive(true);
    }

    private bool HasSpawnEntries(SpawnEntry[] value) =>
        value != null && value.Length > 0;

    private bool IsValidMaximumSpawnInterval(float value) =>
        value >= minimumSpawnInterval;

    private void OnValidate()
    {
        minimumInternalDifficultyFactor = EnemyDifficultyUtility.ClampFactor(
            minimumInternalDifficultyFactor);

        internalDifficultyIncreasePerSecond = Mathf.Max(
            0f,
            internalDifficultyIncreasePerSecond);
    }
}
