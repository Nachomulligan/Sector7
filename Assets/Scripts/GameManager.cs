using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Lógica de partida pura. NO conoce prefabs, spawn points ni formaciones —
/// eso es responsabilidad exclusiva de EnemySpawner. Este script:
///   1) Detiene el spawn unos segundos cuando termina una ronda completa.
///   2) Corta todo si el player muere (Game Over).
///   3) Expone eventos (OnWaveChanged, OnGameOver, OnPauseChanged) para que la UI
///      se entere sin necesidad de que GameManager conozca a UIManagerGameplay.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Spawner")]
    [Tooltip("El EnemySpawner de la escena. GameManager solo llama StopSpawning/StartSpawning.")]
    [SerializeField] private EnemySpawner enemySpawner;

    [Header("Ritmo entre rondas")]
    [Tooltip("Segundos que el spawn queda detenido al terminar una ronda completa antes de arrancar la siguiente. Solo aplica en modo Wave Sequence.")]
    [SerializeField] private float timeBetweenRounds = 3f;

    [Header("Player (opcional, para Game Over)")]
    [SerializeField] private Health playerHealth;

    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; private set; }
    public int CurrentWaveNumber { get; private set; }
    public int TotalWaves => enemySpawner != null ? enemySpawner.WaveCount : 0;

    // ---------- EVENTOS PARA LA UI ----------
    [Tooltip("Se dispara cuando arranca una nueva wave dentro del spawner (índice 1-based, listo para mostrar).")]
    public event Action<int> OnWaveChanged;
    public event Action<int> OnWaveCompleted;
    public event Action OnRoundCompleted;
    public event Action OnGameOver;
    public event Action<bool> OnPauseChanged;

    private Coroutine roundGapRoutine;

    private void OnEnable()
    {
        ServiceLocator.Instance.Register<GameManager>(this);

        if (enemySpawner != null)
        {
            enemySpawner.OnAllWavesFinished += HandleRoundFinished;
            enemySpawner.OnWaveStarted += HandleWaveStarted;
            enemySpawner.OnWaveFinished += HandleWaveFinished;
        }

        if (playerHealth != null)
        {
            playerHealth.OnDeath += HandlePlayerDeath;
        }
    }

    private void OnDisable()
    {
        if (enemySpawner != null)
        {
            enemySpawner.OnAllWavesFinished -= HandleRoundFinished;
            enemySpawner.OnWaveStarted -= HandleWaveStarted;
            enemySpawner.OnWaveFinished -= HandleWaveFinished;
        }

        if (playerHealth != null)
        {
            playerHealth.OnDeath -= HandlePlayerDeath;
        }

        if (ServiceLocator.HasInstance)
        {
            ServiceLocator.Instance.Unregister<GameManager>(this);
        }
    }

    // =====================================================
    // WAVES / RONDAS
    // =====================================================

    private void HandleWaveStarted(int waveIndex, WaveDefinitionSO wave)
    {
        CurrentWaveNumber = waveIndex + 1;
        OnWaveChanged?.Invoke(CurrentWaveNumber);
    }

    private void HandleWaveFinished(int waveIndex, WaveDefinitionSO wave)
    {
        OnWaveCompleted?.Invoke(waveIndex + 1);
    }

    private void HandleRoundFinished()
    {
        if (IsGameOver || enemySpawner == null)
        {
            return;
        }

        OnRoundCompleted?.Invoke();

        if (roundGapRoutine != null)
        {
            StopCoroutine(roundGapRoutine);
        }

        roundGapRoutine = StartCoroutine(RoundGapRoutine());
    }

    private IEnumerator RoundGapRoutine()
    {
        enemySpawner.StopSpawning();

        yield return new WaitForSeconds(timeBetweenRounds);

        if (!IsGameOver)
        {
            enemySpawner.StartSpawning();
        }
    }

    // =====================================================
    // GAME OVER
    // =====================================================

    private void HandlePlayerDeath(GameObject killer)
    {
        IsGameOver = true;

        if (roundGapRoutine != null)
        {
            StopCoroutine(roundGapRoutine);
        }

        if (enemySpawner != null)
        {
            enemySpawner.StopSpawning();
        }

        GameEvents.RaisePlayerDied();
        OnGameOver?.Invoke();
    }

    /// <summary>
    /// Llamado desde el botón "Revive" del panel de Game Over. Reactiva al player,
    /// le devuelve la vida completa y vuelve a arrancar las oleadas desde cero.
    /// </summary>
    public void Revive()
    {
        if (!IsGameOver)
        {
            return;
        }

        IsGameOver = false;
        IsPaused = false;
        Time.timeScale = 1f;

        if (playerHealth != null)
        {
            // Mecha.HandleDeath desactiva el GameObject entero al morir, así que hay
            // que reactivarlo antes de poder resetear su vida.
            playerHealth.gameObject.SetActive(true);
            playerHealth.ResetHealth();
        }

        if (enemySpawner != null)
        {
            enemySpawner.StartSpawning();
        }
    }

    // =====================================================
    // PAUSA (menú)
    // =====================================================

    public void TogglePause() => SetPaused(!IsPaused);
    public void Pause() => SetPaused(true);
    public void Resume() => SetPaused(false);

    private void SetPaused(bool paused)
    {
        if (IsGameOver || IsPaused == paused)
        {
            return;
        }

        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        OnPauseChanged?.Invoke(paused);
    }
}
