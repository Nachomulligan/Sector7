using UnityEngine;

using System;

public class ScoreManager : MonoBehaviour
{
    public event Action<int> OnScoreChanged;

    public int CurrentScore { get; private set; }

    private void OnEnable()
    {
        ServiceLocator.Instance.Register<ScoreManager>(this);
        GameEvents.OnEnemyKilledScored += HandleEnemyKilled;
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
        AddScore(scoreValue);
    }

    public void AddScore(int amount)
    {
        if (amount == 0) return;

        CurrentScore = Mathf.Max(0, CurrentScore + amount);
        OnScoreChanged?.Invoke(CurrentScore);
    }

    public void ResetScore()
    {
        CurrentScore = 0;
        OnScoreChanged?.Invoke(CurrentScore);
    }
}