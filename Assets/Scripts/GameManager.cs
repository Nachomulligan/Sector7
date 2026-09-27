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
[RequireComponent(typeof(RunRewardService))]
[DefaultExecutionOrder(-900)]
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

    [Header("Inicio de Gameplay")]
    [Tooltip("Tiempo máximo de seguridad al abrir Gameplay directamente sin un inventario de sesión.")]
    [Min(0.1f)] [SerializeField] private float loadoutTimeout = 3f;

    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; private set; }
    public int CurrentWaveNumber { get; private set; }
    public int TotalWaves => enemySpawner != null ? enemySpawner.WaveCount : 0;
    public bool IsGameplayReady { get; private set; }

    // ---------- EVENTOS PARA LA UI ----------
    [Tooltip("Se dispara cuando arranca una nueva wave dentro del spawner (índice 1-based, listo para mostrar).")]
    public event Action<int> OnWaveChanged;
    public event Action<int> OnWaveCompleted;
    public event Action OnRoundCompleted;
    public event Action OnGameOver;
    public event Action<bool> OnPauseChanged;

    private Coroutine roundGapRoutine;
    private RunRewardService runRewardService;
    private PlayerInventory playerInventory;
    private Mecha playerMecha;
    private PlayerMechaMovement playerMovement;
    private MechaAbility playerAbility;
    private SpriteRenderer[] playerRenderers = Array.Empty<SpriteRenderer>();

    private void Awake()
    {
        runRewardService = GetComponent<RunRewardService>();
        PrepareGameplayGate();
    }

    private void OnEnable()
    {
        ServiceLocator.Instance.Register<GameManager>(this);
        TryBindPlayerInventory();

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
        if (playerInventory != null)
            playerInventory.OnLoadoutReady -= HandleLoadoutReady;

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

    private void Start()
    {
        StartCoroutine(WaitForLoadout());
    }

    private void PrepareGameplayGate()
    {
        Time.timeScale = 0f;
        if (enemySpawner != null) enemySpawner.SetStartupBlocked(true);
        if (playerHealth == null) return;

        playerMecha = playerHealth.GetComponent<Mecha>();
        playerMovement = playerHealth.GetComponent<PlayerMechaMovement>();
        playerAbility = playerHealth.GetComponent<MechaAbility>();
        playerRenderers = playerHealth.GetComponentsInChildren<SpriteRenderer>(true);

        if (playerMecha != null) playerMecha.enabled = false;
        if (playerMovement != null) playerMovement.enabled = false;
        if (playerAbility != null) playerAbility.enabled = false;
        foreach (SpriteRenderer playerRenderer in playerRenderers)
            playerRenderer.enabled = false;
    }

    private bool TryBindPlayerInventory()
    {
        if (playerInventory != null) return true;
        if (!ServiceLocator.Instance.TryGet(out playerInventory)) return false;
        playerInventory.OnLoadoutReady += HandleLoadoutReady;
        return true;
    }

    private IEnumerator WaitForLoadout()
    {
        float deadline = Time.realtimeSinceStartup + loadoutTimeout;
        while (!IsGameplayReady && Time.realtimeSinceStartup < deadline)
        {
            TryBindPlayerInventory();
            yield return null;
        }

        if (!IsGameplayReady)
        {
            Debug.LogWarning("[GameManager] No llegó OnLoadoutReady. Se inicia con el loadout disponible en escena.", this);
            BeginGameplay();
        }
    }

    private void HandleLoadoutReady(bool applied, string status)
    {
        if (!applied)
        {
            Debug.LogError($"[GameManager] El loadout no pudo aplicarse: {status}", this);
            return;
        }

        BeginGameplay();
    }

    private void BeginGameplay()
    {
        if (IsGameplayReady) return;
        IsGameplayReady = true;

        if (playerMecha != null) playerMecha.enabled = true;
        if (playerMovement != null) playerMovement.enabled = true;
        if (playerAbility != null) playerAbility.enabled = true;
        foreach (SpriteRenderer playerRenderer in playerRenderers)
            playerRenderer.enabled = true;

        if (enemySpawner != null)
        {
            enemySpawner.SetStartupBlocked(false);
            enemySpawner.StartSpawning();
        }

        Time.timeScale = 1f;
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

        Time.timeScale = 0f;
        GameEvents.RaisePlayerDied();
        runRewardService.SettleRun();
        OnGameOver?.Invoke();
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
