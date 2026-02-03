using UnityEngine;
using TMPro;

public class DifficultyTextAnimator : MonoBehaviour
{
    [Header("Text Components")]
    [SerializeField] private TextMeshProUGUI difficultyText;
    [SerializeField] private TextMeshProUGUI difficultyTextNext;

    [Header("Dependencies")]
    [SerializeField] private DifficultyColorProvider colorProvider;

    [Header("Animation Settings")]
    [SerializeField] private float textRotationAmount = 30f;

    private RectTransform difficultyTextRect;
    private RectTransform difficultyTextNextRect;
    private Vector2 textOriginalPosition;
    private Vector3 textOriginalScale;

    public void Initialize()
    {
        // Get RectTransform of difficulty texts
        if (difficultyText != null)
        {
            difficultyTextRect = difficultyText.GetComponent<RectTransform>();
            if (difficultyTextRect != null)
            {
                textOriginalPosition = difficultyTextRect.anchoredPosition;
                textOriginalScale = difficultyTextRect.localScale;
            }
        }

        if (difficultyTextNext != null)
        {
            difficultyTextNextRect = difficultyTextNext.GetComponent<RectTransform>();
            if (difficultyTextNextRect != null)
            {
                difficultyTextNextRect.anchoredPosition = textOriginalPosition;
                difficultyTextNextRect.localScale = textOriginalScale;
            }
        }
    }

    public void UpdateTextRotation(float sliderValue)
    {
        if (difficultyTextRect == null || colorProvider == null)
            return;

        // Determine which two difficulties we're between
        int lowerDifficulty = Mathf.FloorToInt(sliderValue);
        int upperDifficulty = Mathf.CeilToInt(sliderValue);
        float t = sliderValue - lowerDifficulty; // 0 to 1 between two difficulties

        // Get difficulty names and colors
        string lowerName = GetDifficultyName(lowerDifficulty);
        string upperName = GetDifficultyName(upperDifficulty);
        Color lowerColor = colorProvider.GetColorForDifficulty(lowerDifficulty);
        Color upperColor = colorProvider.GetColorForDifficulty(upperDifficulty);

        // Calculate rotation progress (0 = fully showing lower, 1 = fully showing upper)
        float rotationProgress = t;

        // Update main text (current difficulty rotating away upward)
        if (difficultyText != null)
        {
            difficultyText.text = lowerName;

            // Scale: compress vertically as rotation progresses
            float scaleY = Mathf.Cos(rotationProgress * Mathf.PI * 0.5f);
            difficultyTextRect.localScale = new Vector3(textOriginalScale.x, textOriginalScale.y * Mathf.Abs(scaleY), textOriginalScale.z);

            // Position: move up as it rotates
            float offsetY = rotationProgress * textRotationAmount;
            difficultyTextRect.anchoredPosition = textOriginalPosition + new Vector2(0, offsetY);

            // Fade out
            Color color = lowerColor;
            color.a = 1f - rotationProgress;
            difficultyText.color = color;
        }

        // Update next text (next difficulty rotating in from below)
        if (difficultyTextNext != null && difficultyTextNextRect != null)
        {
            difficultyTextNext.text = upperName;

            // Scale: expand vertically as it appears
            float scaleY = Mathf.Sin(rotationProgress * Mathf.PI * 0.5f);
            difficultyTextNextRect.localScale = new Vector3(textOriginalScale.x, textOriginalScale.y * scaleY, textOriginalScale.z);

            // Position: move from below to center
            float offsetY = (rotationProgress - 1f) * textRotationAmount;
            difficultyTextNextRect.anchoredPosition = textOriginalPosition + new Vector2(0, offsetY);

            // Fade in
            Color color = upperColor;
            color.a = rotationProgress;
            difficultyTextNext.color = color;
        }
    }

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
}
