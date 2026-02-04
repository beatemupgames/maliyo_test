using UnityEngine;
using TMPro;

public class DifficultyTextAnimator : MonoBehaviour
{
    #region Serialized Fields

    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI difficultyText;
    [SerializeField] private TextMeshProUGUI difficultyTextNext;

    [Header("Dependencies")]
    [SerializeField] private DifficultyColorProvider colorProvider;

    [Header("Animation Settings")]
    [SerializeField] private float textRotationAmount = 30f;

    #endregion

    #region Private Fields

    private RectTransform difficultyTextRect;
    private RectTransform difficultyTextNextRect;
    private Vector2 textOriginalPosition;
    private Vector3 textOriginalScale;

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes the text animator by caching RectTransforms and storing original transform values.
    /// Sets up both the current and next difficulty text components for animation.
    /// </summary>
    public void Initialize()
    {
        // Cache RectTransform and store original values for the current difficulty text
        if (difficultyText != null)
        {
            difficultyTextRect = difficultyText.GetComponent<RectTransform>();
            if (difficultyTextRect != null)
            {
                // Store original position and scale for animation reference
                textOriginalPosition = difficultyTextRect.anchoredPosition;
                textOriginalScale = difficultyTextRect.localScale;
            }
        }

        // Cache RectTransform and set initial values for the next difficulty text
        if (difficultyTextNext != null)
        {
            difficultyTextNextRect = difficultyTextNext.GetComponent<RectTransform>();
            if (difficultyTextNextRect != null)
            {
                // Match the original position and scale
                difficultyTextNextRect.anchoredPosition = textOriginalPosition;
                difficultyTextNextRect.localScale = textOriginalScale;
            }
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Updates the text rotation animation based on the slider value.
    /// Creates a smooth transition effect between difficulty levels with rotation, scale, and fade animations.
    /// </summary>
    /// <param name="sliderValue">The current slider value (0-2), can be fractional</param>
    public void UpdateTextRotation(float sliderValue)
    {
        // Early exit if required components are missing
        if (difficultyTextRect == null || colorProvider == null)
            return;

        // Determine which two difficulty levels we're transitioning between
        int lowerDifficulty = Mathf.FloorToInt(sliderValue);
        int upperDifficulty = Mathf.CeilToInt(sliderValue);
        float t = sliderValue - lowerDifficulty; // Normalized value (0 to 1) between difficulties

        // Get difficulty names and colors for both levels
        string lowerName = GetDifficultyName(lowerDifficulty);
        string upperName = GetDifficultyName(upperDifficulty);
        Color lowerColor = colorProvider.GetColorForDifficulty(lowerDifficulty);
        Color upperColor = colorProvider.GetColorForDifficulty(upperDifficulty);

        // Calculate animation progress (0 = showing lower difficulty, 1 = showing upper difficulty)
        float rotationProgress = t;

        // Animate the current difficulty text (rotating out upward)
        if (difficultyText != null)
        {
            // Set text content to current difficulty
            difficultyText.text = lowerName;

            // Apply vertical compression using cosine for smooth scaling
            float scaleY = Mathf.Cos(rotationProgress * Mathf.PI * 0.5f);
            difficultyTextRect.localScale = new Vector3(textOriginalScale.x, textOriginalScale.y * Mathf.Abs(scaleY), textOriginalScale.z);

            // Move upward as animation progresses
            float offsetY = rotationProgress * textRotationAmount;
            difficultyTextRect.anchoredPosition = textOriginalPosition + new Vector2(0, offsetY);

            // Fade out as it moves up
            Color color = lowerColor;
            color.a = 1f - rotationProgress;
            difficultyText.color = color;
        }

        // Animate the next difficulty text (rotating in from below)
        if (difficultyTextNext != null && difficultyTextNextRect != null)
        {
            // Set text content to next difficulty
            difficultyTextNext.text = upperName;

            // Apply vertical expansion using sine for smooth scaling
            float scaleY = Mathf.Sin(rotationProgress * Mathf.PI * 0.5f);
            difficultyTextNextRect.localScale = new Vector3(textOriginalScale.x, textOriginalScale.y * scaleY, textOriginalScale.z);

            // Move from below to center position
            float offsetY = (rotationProgress - 1f) * textRotationAmount;
            difficultyTextNextRect.anchoredPosition = textOriginalPosition + new Vector2(0, offsetY);

            // Fade in as it moves up
            Color color = upperColor;
            color.a = rotationProgress;
            difficultyTextNext.color = color;
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Converts a difficulty index to its corresponding display name.
    /// </summary>
    /// <param name="difficultyIndex">The difficulty level (0 = Easy, 1 = Medium, 2 = Hard)</param>
    /// <returns>The difficulty name as a string, defaults to "EASY" for invalid indices</returns>
    private string GetDifficultyName(int difficultyIndex)
    {
        switch (difficultyIndex)
        {
            case 0: return "EASY";
            case 1: return "MEDIUM";
            case 2: return "HARD";
            default: return "EASY";
        }
    }

    #endregion
}
