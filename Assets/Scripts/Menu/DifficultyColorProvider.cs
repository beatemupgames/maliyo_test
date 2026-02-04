using UnityEngine;

public class DifficultyColorProvider : MonoBehaviour
{
    [Header("Difficulty Colors")]
    [SerializeField] private DifficultyColorsConfig colorsConfig;

    /// <summary>
    /// Gets the interpolated color based on slider value (0 = Easy, 1 = Medium, 2 = Hard)
    /// </summary>
    public Color GetColorForSliderValue(float sliderValue)
    {
        if (colorsConfig == null)
        {
            return Color.white;
        }

        // Interpolate color based on slider position
        if (sliderValue <= 1f)
        {
            // Between Easy (0) and Medium (1) - interpolate between green and orange
            return Color.Lerp(colorsConfig.EasyColor, colorsConfig.MediumColor, sliderValue);
        }
        else
        {
            // Between Medium (1) and Hard (2) - interpolate between orange and red
            return Color.Lerp(colorsConfig.MediumColor, colorsConfig.HardColor, sliderValue - 1f);
        }
    }

    /// <summary>
    /// Gets the color for a specific difficulty index (0 = Easy, 1 = Medium, 2 = Hard)
    /// </summary>
    public Color GetColorForDifficulty(int difficultyIndex)
    {
        if (colorsConfig == null)
        {
            return Color.white;
        }

        return colorsConfig.GetColorForDifficulty(difficultyIndex);
    }

    public Color EasyColor => colorsConfig != null ? colorsConfig.EasyColor : Color.white;
    public Color MediumColor => colorsConfig != null ? colorsConfig.MediumColor : Color.white;
    public Color HardColor => colorsConfig != null ? colorsConfig.HardColor : Color.white;
}
