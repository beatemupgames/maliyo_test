using UnityEngine;
using TMPro;

public class GameHUDManager : MonoBehaviour
{
    #region Serialized Fields

    [Header("Mode Block (Current Score & Difficulty)")]
    [SerializeField] private TextMeshProUGUI modeText;
    [SerializeField] private TextMeshProUGUI modeNumber;

    [Header("All Time Block (Best Score)")]
    [SerializeField] private TextMeshProUGUI allTimeNumber;

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes the HUD with the current difficulty and best score.
    /// Sets up all UI text elements with their initial values.
    /// </summary>
    /// <param name="difficulty">The current difficulty level</param>
    /// <param name="bestScore">The best score of all time for this difficulty</param>
    public void Initialize(GameManager.Difficulty difficulty, int bestScore)
    {
        // Update difficulty mode display
        UpdateMode(difficulty);

        // Update best score display
        UpdateBestScore(bestScore);

        // Initialize current score to 0
        UpdateCurrentScore(0);
    }

    #endregion

    #region UI Update Methods

    /// <summary>
    /// Updates the difficulty mode text display.
    /// Converts the difficulty enum to a user-friendly text string.
    /// </summary>
    /// <param name="difficulty">The difficulty level to display</param>
    public void UpdateMode(GameManager.Difficulty difficulty)
    {
        if (modeText != null)
        {
            // Convert difficulty enum to display text
            string modeTextString = difficulty switch
            {
                GameManager.Difficulty.Easy => "EASY MODE",
                GameManager.Difficulty.Medium => "MEDIUM MODE",
                GameManager.Difficulty.Hard => "HARD MODE",
                _ => "MEDIUM MODE"
            };

            // Update the mode text UI element
            modeText.text = modeTextString;
        }
    }

    /// <summary>
    /// Updates the current score display in the mode block.
    /// Called whenever the player completes a round.
    /// </summary>
    /// <param name="score">The current player score to display</param>
    public void UpdateCurrentScore(int score)
    {
        if (modeNumber != null)
        {
            // Update current score text
            modeNumber.text = score.ToString();
        }
    }

    /// <summary>
    /// Updates the best score (all-time) display.
    /// Shows the highest score ever achieved for the current difficulty.
    /// </summary>
    /// <param name="bestScore">The best score to display</param>
    public void UpdateBestScore(int bestScore)
    {
        if (allTimeNumber != null)
        {
            // Update best score text
            allTimeNumber.text = bestScore.ToString();
        }
    }

    /// <summary>
    /// Updates both current and best scores simultaneously.
    /// Convenience method for updating both score displays at once.
    /// </summary>
    /// <param name="currentScore">The current player score</param>
    /// <param name="bestScore">The best score of all time</param>
    public void UpdateScores(int currentScore, int bestScore)
    {
        // Update current score
        UpdateCurrentScore(currentScore);

        // Update best score
        UpdateBestScore(bestScore);
    }

    #endregion
}
