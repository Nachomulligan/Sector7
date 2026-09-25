using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class EnemySpawnEntry
{
    public GameObject enemyPrefab;

    [Tooltip("Peso relativo al sortear qué enemigo spawnear. Más alto = más frecuente.")]
    public float weight = 1f;
}


public class GameManager : MonoBehaviour
{
    [Header("Enemigos")]
    [SerializeField] private List<EnemySpawnEntry> enemyPool = new List<EnemySpawnEntry>();

    [Header("Puntos de spawn")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Configuración de oleadas")]
    [Tooltip("Cantidad de enemigos en la oleada 1.")]
    [SerializeField] private int baseEnemiesPerWave = 5;

    [Tooltip("Multiplicador de crecimiento por oleada. 1.5 = cada oleada tiene 50% más enemigos que la anterior.")]
    [SerializeField] private float enemiesPerWaveGrowth = 1.15f;

    [Tooltip("Tiempo entre cada spawn individual dentro de una oleada.")]
    [SerializeField] private float timeBetweenSpawns = 0.5f;

    [Tooltip("Tiempo de descanso entre que termina una oleada y arranca la siguiente.")]
    [SerializeField] private float timeBetweenWaves = 3f;

    [Header("Dificultad (opcional)")]
    [Tooltip("Vida extra que se le suma a cada enemigo, multiplicada por (oleada - 1). Poné 0 para desactivar.")]
    [SerializeField] private int bonusHealthPerWave = 5;

    [Header("Player (opcional, para Game Over)")]
    [Tooltip("Si lo asignás, el GameManager frena el spawneo cuando el player muere.")]
    [SerializeField] private Health playerHealth;

    public int CurrentWave { get; private set; }
    public bool IsGameOver { get; private set; }

    private int aliveEnemies;
    private Coroutine wavesRoutine;

    private void OnEnable()
    {
        GameEvents.OnEnemyDied += HandleEnemyDied;

        if (playerHealth != null)
        {
            playerHealth.OnDeath += HandlePlayerDeath;
        }
    }

    private void OnDisable()
    {
        GameEvents.OnEnemyDied -= HandleEnemyDied;

        if (playerHealth != null)
        {
            playerHealth.OnDeath -= HandlePlayerDeath;
        }
    }

    private void Start()
    {
        wavesRoutine = StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        while (!IsGameOver)
        {
            CurrentWave++;
            GameEvents.RaiseWaveStarted(CurrentWave);

            int enemyCount = Mathf.CeilToInt(
                baseEnemiesPerWave * Mathf.Pow(enemiesPerWaveGrowth, CurrentWave - 1)
            );

            yield return StartCoroutine(SpawnWave(enemyCount));

            yield return new WaitUntil(() => aliveEnemies <= 0 || IsGameOver);

            if (IsGameOver)
            {
                yield break;
            }

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private IEnumerator SpawnWave(int enemyCount)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            if (IsGameOver)
            {
                yield break;
            }

            SpawnEnemy();
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPool.Count == 0 || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("GameManager: falta asignar enemyPool o spawnPoints en el Inspector.");
            return;
        }

        GameObject prefab = PickWeightedEnemy();
        if (prefab == null) return;

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject instance = PoolManager.Instance.Spawn(prefab, point.position, point.rotation);

        aliveEnemies++;

        if (instance.TryGetComponent(out Health enemyHealth))
        {

            enemyHealth.SetBonusMaxHealth(bonusHealthPerWave * (CurrentWave - 1));
            enemyHealth.ResetHealth();
        }
    }

    private GameObject PickWeightedEnemy()
    {
        float totalWeight = 0f;
        foreach (var entry in enemyPool)
        {
            totalWeight += entry.weight;
        }

        if (totalWeight <= 0f)
        {
            return enemyPool[0].enemyPrefab;
        }

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var entry in enemyPool)
        {
            cumulative += entry.weight;
            if (roll <= cumulative)
            {
                return entry.enemyPrefab;
            }
        }

        return enemyPool[enemyPool.Count - 1].enemyPrefab;
    }

    private void HandleEnemyDied(GameObject enemyInstance)
    {

        aliveEnemies = Mathf.Max(0, aliveEnemies - 1);
    }

    private void HandlePlayerDeath(GameObject killer)
    {
        IsGameOver = true;

        if (wavesRoutine != null)
        {
            StopCoroutine(wavesRoutine);
        }

        GameEvents.RaisePlayerDied();
    }
}