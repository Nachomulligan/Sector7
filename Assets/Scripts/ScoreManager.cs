using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public event Action<int> OnScoreChanged;

    public int CurrentScore { get; private set; }

    private void OnEnable()
    {
        ServiceLocator.Instance.Register<ScoreManager>(this);
        GameEvents.OnEnemyKilledScored += HandleEnemyKilled;

        Debug.Log("[ScoreManager] Registrado y suscripto a GameEvents.OnEnemyKilledScored.");
    }

    private void OnDisable()
    {
        GameEvents.OnEnemyKilledScored -= HandleEnemyKilled;

        if (ServiceLocator.HasInstance)
        {
            ServiceLocator.Instance.Unregister<ScoreManager>(this);
        }
    }

    private void HandleEnemyKilled(int scoreValue)
    {
        Debug.Log($"[ScoreManager] Recibí GameEvents.OnEnemyKilledScored con valor {scoreValue}.");
        AddScore(scoreValue);
    }

    public void AddScore(int amount)
    {
        if (amount == 0) return;

        CurrentScore = Mathf.Max(0, CurrentScore + amount);
        Debug.Log($"[ScoreManager] Score actualizado a {CurrentScore}.");
        OnScoreChanged?.Invoke(CurrentScore);
    }

    public void ResetScore()
    {
        CurrentScore = 0;
        OnScoreChanged?.Invoke(CurrentScore);
    }
}