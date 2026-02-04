using UnityEngine;
using TMPro;

public class GameHUDManager : MonoBehaviour
{
    [Header("Mode Block (Current Score & Difficulty)")]
    [SerializeField] private TextMeshProUGUI modeText;
    [SerializeField] private TextMeshProUGUI modeNumber;

    [Header("All Time Block (Best Score)")]
    [SerializeField] private TextMeshProUGUI allTimeNumber;

    public void Initialize(GameManager.Difficulty difficulty, int bestScore)
    {
        UpdateMode(difficulty);
        UpdateBestScore(bestScore);
        UpdateCurrentScore(0);
    }

    public void UpdateMode(GameManager.Difficulty difficulty)
    {
        if (modeText != null)
        {
            string modeTextString = difficulty switch
            {
                GameManager.Difficulty.Easy => "EASY MODE",
                GameManager.Difficulty.Medium => "MEDIUM MODE",
                GameManager.Difficulty.Hard => "HARD MODE",
                _ => "MEDIUM MODE"
            };
            modeText.text = modeTextString;
        }
    }

    public void UpdateCurrentScore(int score)
    {
        if (modeNumber != null)
        {
            modeNumber.text = score.ToString();
        }
    }

    public void UpdateBestScore(int bestScore)
    {
        if (allTimeNumber != null)
        {
            allTimeNumber.text = bestScore.ToString();
        }
    }

    public void UpdateScores(int currentScore, int bestScore)
    {
        UpdateCurrentScore(currentScore);
        UpdateBestScore(bestScore);
    }
}
