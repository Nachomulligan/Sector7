using UnityEngine;

using System;

public static class GameEvents
{
    /// <summary>Se dispara cuando un enemigo muere, pasando el GameObject del enemigo.</summary>
    public static event Action<GameObject> OnEnemyDied;

    /// <summary>Se dispara cuando un enemigo muere, pasando el puntaje que otorga.</summary>
    public static event Action<int> OnEnemyKilledScored;

    /// <summary>Se dispara al iniciar una nueva oleada, pasando el número de oleada (1, 2, 3...).</summary>
    public static event Action<int> OnWaveStarted;

    /// <summary>Se dispara cuando el player muere.</summary>
    public static event Action OnPlayerDied;

    public static void RaiseEnemyDied(GameObject enemy) => OnEnemyDied?.Invoke(enemy);
    public static void RaiseEnemyKilledScored(int scoreValue) => OnEnemyKilledScored?.Invoke(scoreValue);
    public static void RaiseWaveStarted(int waveNumber) => OnWaveStarted?.Invoke(waveNumber);
    public static void RaisePlayerDied() => OnPlayerDied?.Invoke();
}

