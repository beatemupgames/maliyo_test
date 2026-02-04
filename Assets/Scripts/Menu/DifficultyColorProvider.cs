using UnityEngine;

/// <summary>
/// Provides difficulty colors for UI elements based on game difficulty levels.
/// Acts as a bridge between the UI and the DifficultyColorsConfig ScriptableObject.
/// </summary>
public class DifficultyColorProvider : MonoBehaviour
{
    #region Variables

    [Header("Difficulty Colors")]
    [SerializeField] private DifficultyColorsConfig colorsConfig; // Reference to the difficulty colors configuration

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets the interpolated color based on slider value.
    /// Smoothly transitions between difficulty colors as the slider moves.
    /// </summary>
    /// <param name="sliderValue">Slider position (0 = Easy, 1 = Medium, 2 = Hard)</param>
    /// <returns>Interpolated color between difficulty levels</returns>
    public Color GetColorForSliderValue(float sliderValue)
    {
        // Return white as fallback if config is missing
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
    /// Gets the exact color for a specific difficulty level.
    /// Returns the color without interpolation.
    /// </summary>
    /// <param name="difficultyIndex">Difficulty level (0 = Easy, 1 = Medium, 2 = Hard)</param>
    /// <returns>Color assigned to the specified difficulty level</returns>
    public Color GetColorForDifficulty(int difficultyIndex)
    {
        // Return white as fallback if config is missing
        if (colorsConfig == null)
        {
            return Color.white;
        }

        // Delegate to the config's method
        return colorsConfig.GetColorForDifficulty(difficultyIndex);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the color assigned to Easy difficulty.
    /// </summary>
    public Color EasyColor => colorsConfig != null ? colorsConfig.EasyColor : Color.white;

    /// <summary>
    /// Gets the color assigned to Medium difficulty.
    /// </summary>
    public Color MediumColor => colorsConfig != null ? colorsConfig.MediumColor : Color.white;

    /// <summary>
    /// Gets the color assigned to Hard difficulty.
    /// </summary>
    public Color HardColor => colorsConfig != null ? colorsConfig.HardColor : Color.white;

    #endregion
}
