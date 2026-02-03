using UnityEngine;

public class DifficultyColorProvider : MonoBehaviour
{
    [Header("Difficulty Colors")]
    [SerializeField] private Color easyDifficultyColor = new Color(0.184f, 0.718f, 0.255f); // #2FB741
    [SerializeField] private Color mediumDifficultyColor = new Color(0.992f, 0.655f, 0.016f); // #FDA704
    [SerializeField] private Color hardDifficultyColor = new Color(0.996f, 0.373f, 0.337f); // #FE5F56

    /// <summary>
    /// Gets the interpolated color based on slider value (0 = Easy, 1 = Medium, 2 = Hard)
    /// </summary>
    public Color GetColorForSliderValue(float sliderValue)
    {
        // Interpolate color based on slider position
        if (sliderValue <= 1f)
        {
            // Between Easy (0) and Medium (1) - interpolate between green and orange
            return Color.Lerp(easyDifficultyColor, mediumDifficultyColor, sliderValue);
        }
        else
        {
            // Between Medium (1) and Hard (2) - interpolate between orange and red
            return Color.Lerp(mediumDifficultyColor, hardDifficultyColor, sliderValue - 1f);
        }
    }

    /// <summary>
    /// Gets the color for a specific difficulty index (0 = Easy, 1 = Medium, 2 = Hard)
    /// </summary>
    public Color GetColorForDifficulty(int difficultyIndex)
    {
        switch (difficultyIndex)
        {
            case 0: return easyDifficultyColor;
            case 1: return mediumDifficultyColor;
            case 2: return hardDifficultyColor;
            default: return easyDifficultyColor;
        }
    }

    public Color EasyColor => easyDifficultyColor;
    public Color MediumColor => mediumDifficultyColor;
    public Color HardColor => hardDifficultyColor;
}
