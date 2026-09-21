using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public enum EnemySpawnerMode
{
    EndlessRandom,
    WaveSequence
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Modo")]
    [SerializeField] private EnemySpawnerMode mode = EnemySpawnerMode.EndlessRandom;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loopSequence;

    [Header("Límites horizontales")]
    [FormerlySerializedAs("minX")]
    [SerializeField] private float minSpawnX = -2.5f;
    [FormerlySerializedAs("maxX")]
    [SerializeField] private float maxSpawnX = 2.5f;

    [Header("Control de población")]
    [Min(1)] [SerializeField] private int maxActiveEnemies = 30;
    [Tooltip("Detiene nuevas apariciones cuando no hay un jugador vivo registrado.")]
    [SerializeField] private bool pauseWithoutLivingPlayer = true;

    [Header("Modo Endless Random")]
    [Tooltip("Tabla ponderada principal. Si está vacía, se usa la lista de prefabs o el prefab legacy.")]
    [SerializeField] private EnemySpawnTableSO endlessEnemyTable;
    [SerializeField] private List<GameObject> endlessEnemyPrefabs = new List<GameObject>();
    [SerializeField] private List<FormationPresetSO> endlessFormations = new List<FormationPresetSO>();
    [SerializeField] private Vector2 endlessAnchor = new Vector2(0f, 6f);
    [FormerlySerializedAs("spawnInterval")]
    [Min(0.05f)] [SerializeField] private float intervalBetweenFormations = 1.5f;

    [Header("Modo Wave Sequence")]
    [SerializeField] private List<WaveDefinitionSO> waves = new List<WaveDefinitionSO>();

    [Header("Compatibilidad")]
    [FormerlySerializedAs("enemyPrefab")]
    [Tooltip("Mantiene funcionando escenas anteriores. Para contenido nuevo usar tabla o lista.")]
    [SerializeField] private GameObject legacyEnemyPrefab;
    [FormerlySerializedAs("spawnY")]
    [SerializeField] private float legacySpawnY = 6f;

    [Header("Eventos para Inspector")]
    [SerializeField] private UnityEvent<int> onWaveStarted = new UnityEvent<int>();
    [SerializeField] private UnityEvent<int> onWaveFinished = new UnityEvent<int>();
    [SerializeField] private UnityEvent onAllWavesFinished = new UnityEvent();

    public event Action<int, WaveDefinitionSO> OnWaveStarted;
    public event Action<int, WaveDefinitionSO> OnWaveFinished;
    public event Action OnAllWavesFinished;

    public bool IsRunning => spawnRoutine != null;
    public int CurrentWaveIndex { get; private set; } = -1;

    private readonly List<Vector2> formationOffsets = new List<Vector2>();
    private readonly HashSet<EnemyMecha> activeEnemies = new HashSet<EnemyMecha>();
    private readonly HashSet<EnemyMecha> currentWaveEnemies = new HashSet<EnemyMecha>();
    private Coroutine spawnRoutine;

    private void Start()
    {
        if (playOnStart)
        {
            StartSpawning();
        }
    }

    private void OnDisable()
    {
        StopSpawning();
    }

    public void StartSpawning()
    {
        StopSpawning();
        spawnRoutine = StartCoroutine(mode == EnemySpawnerMode.WaveSequence
            ? RunWaveSequence()
            : RunEndlessRandom());
    }

    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        UnsubscribeTrackedEnemies();
        CurrentWaveIndex = -1;
    }

    public void PlayWave(int waveIndex)
    {
        if (waveIndex < 0 || waveIndex >= waves.Count || waves[waveIndex] == null)
        {
            Debug.LogWarning($"{name}: wave index {waveIndex} inválido.", this);
            return;
        }

        StopSpawning();
        spawnRoutine = StartCoroutine(RunSingleWave(waveIndex, waves[waveIndex], true));
    }

    private IEnumerator RunEndlessRandom()
    {
        CurrentWaveIndex = -1;

        while (true)
        {
            FormationPresetSO formation = GetRandomFormation();

            if (formation != null)
            {
                formation.FillOffsets(formationOffsets, formation.ShouldMirror());
                yield return SpawnOffsets(
                    formationOffsets,
                    endlessAnchor,
                    formation.SpawnInterval,
                    0f,
                    GetEndlessEnemyPrefab
                );
            }
            else
            {
                yield return new WaitUntil(CanSpawnNextEnemy);
                GameObject prefab = GetEndlessEnemyPrefab();

                if (prefab != null)
                {
                    SpawnEnemy(prefab, new Vector2(UnityEngine.Random.Range(minSpawnX, maxSpawnX), legacySpawnY), false);
                }
            }

            yield return new WaitForSeconds(intervalBetweenFormations);
        }
    }

    private IEnumerator RunWaveSequence()
    {
        if (waves.Count == 0)
        {
            Debug.LogWarning($"{name}: no hay Wave Definitions asignadas.", this);
            spawnRoutine = null;
            yield break;
        }

        do
        {
            for (int i = 0; i < waves.Count; i++)
            {
                if (waves[i] != null)
                {
                    yield return RunSingleWave(i, waves[i], false);
                }
            }

            OnAllWavesFinished?.Invoke();
            onAllWavesFinished?.Invoke();
        }
        while (loopSequence);

        CurrentWaveIndex = -1;
        spawnRoutine = null;
    }

    private IEnumerator RunSingleWave(int waveIndex, WaveDefinitionSO wave, bool finishRoutine)
    {
        CurrentWaveIndex = waveIndex;
        currentWaveEnemies.Clear();

        OnWaveStarted?.Invoke(waveIndex, wave);
        onWaveStarted?.Invoke(waveIndex);

        foreach (WaveSpawnInstruction instruction in wave.SpawnInstructions)
        {
            if (instruction == null)
            {
                continue;
            }

            if (instruction.DelayBeforeSpawn > 0f)
            {
                yield return new WaitForSeconds(instruction.DelayBeforeSpawn);
            }

            if (instruction.Formation != null)
            {
                instruction.Formation.FillOffsets(formationOffsets, instruction.Mirror);
            }
            else
            {
                formationOffsets.Clear();
                formationOffsets.Add(Vector2.zero);
            }

            yield return SpawnOffsets(
                formationOffsets,
                instruction.Anchor,
                instruction.SpawnInterval,
                instruction.RandomHorizontalOffset,
                instruction.GetEnemyPrefab
            );
        }

        if (wave.WaitForAllEnemies)
        {
            yield return new WaitUntil(() => currentWaveEnemies.Count == 0);
        }

        if (wave.DelayAfterWave > 0f)
        {
            yield return new WaitForSeconds(wave.DelayAfterWave);
        }

        OnWaveFinished?.Invoke(waveIndex, wave);
        onWaveFinished?.Invoke(waveIndex);

        if (finishRoutine)
        {
            CurrentWaveIndex = -1;
            spawnRoutine = null;
        }
    }

    private IEnumerator SpawnOffsets(
        List<Vector2> offsets,
        Vector2 anchor,
        float spawnInterval,
        float randomHorizontalOffset,
        Func<GameObject> prefabProvider)
    {
        foreach (Vector2 offset in offsets)
        {
            yield return new WaitUntil(CanSpawnNextEnemy);

            GameObject prefab = prefabProvider();

            if (prefab == null)
            {
                Debug.LogWarning($"{name}: no se pudo resolver un prefab enemigo.", this);
                continue;
            }

            float randomX = randomHorizontalOffset > 0f
                ? UnityEngine.Random.Range(-randomHorizontalOffset, randomHorizontalOffset)
                : 0f;
            Vector2 position = anchor + offset + new Vector2(randomX, 0f);
            position.x = Mathf.Clamp(position.x, minSpawnX, maxSpawnX);
            SpawnEnemy(prefab, position, mode == EnemySpawnerMode.WaveSequence);

            if (spawnInterval > 0f)
            {
                yield return new WaitForSeconds(spawnInterval);
            }
        }
    }

    private void SpawnEnemy(GameObject prefab, Vector2 position, bool trackForWave)
    {
        if (PoolManager.Instance == null)
        {
            Debug.LogError($"{name}: falta PoolManager en la escena.", this);
            return;
        }

        GameObject instance = PoolManager.Instance.Spawn(prefab, position, Quaternion.identity);

        if (!instance.TryGetComponent(out EnemyMecha enemy))
        {
            return;
        }

        if (activeEnemies.Add(enemy))
        {
            enemy.OnReturnedToPool += HandleEnemyReturnedToPool;
        }

        if (trackForWave)
        {
            currentWaveEnemies.Add(enemy);
        }
    }

    private void HandleEnemyReturnedToPool(EnemyMecha enemy)
    {
        enemy.OnReturnedToPool -= HandleEnemyReturnedToPool;
        activeEnemies.Remove(enemy);
        currentWaveEnemies.Remove(enemy);
    }

    private void UnsubscribeTrackedEnemies()
    {
        foreach (EnemyMecha enemy in activeEnemies)
        {
            if (enemy != null)
            {
                enemy.OnReturnedToPool -= HandleEnemyReturnedToPool;
            }
        }

        activeEnemies.Clear();
        currentWaveEnemies.Clear();
    }

    private bool CanSpawnNextEnemy()
    {
        if (activeEnemies.Count >= maxActiveEnemies)
        {
            return false;
        }

        if (!pauseWithoutLivingPlayer)
        {
            return true;
        }

        return ServiceLocator.Instance.TryGet<IPlayerTarget>(out IPlayerTarget player) && player.IsAlive;
    }

    private FormationPresetSO GetRandomFormation()
    {
        if (endlessFormations.Count == 0)
        {
            return null;
        }

        return endlessFormations[UnityEngine.Random.Range(0, endlessFormations.Count)];
    }

    private GameObject GetEndlessEnemyPrefab()
    {
        if (endlessEnemyTable != null)
        {
            GameObject weightedPrefab = endlessEnemyTable.GetRandomPrefab();

            if (weightedPrefab != null)
            {
                return weightedPrefab;
            }
        }

        if (endlessEnemyPrefabs.Count > 0)
        {
            return endlessEnemyPrefabs[UnityEngine.Random.Range(0, endlessEnemyPrefabs.Count)];
        }

        return legacyEnemyPrefab;
    }
}
