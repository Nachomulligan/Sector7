using UnityEngine;

[CreateAssetMenu(menuName = "Sector7/Gameplay/Score Reward Config", fileName = "ScoreRewardConfig")]
public sealed class ScoreRewardConfigSO : ScriptableObject
{
    [Min(1)] [SerializeField] private int scorePerStep = 100;
    [Min(1)] [SerializeField] private int creditsPerStep = 10;

    public int CalculateCredits(int score)
    {
        if (score <= 0) return 0;
        return Mathf.FloorToInt(score / (float)scorePerStep) * creditsPerStep;
    }
}
