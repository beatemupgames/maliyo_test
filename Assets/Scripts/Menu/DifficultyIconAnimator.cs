using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Animates the difficulty icon's facial features based on the selected difficulty level.
/// Handles animations for mouth, cheekbones, eyes, eyebrows, horns, and color changes.
/// </summary>
public class DifficultyIconAnimator : MonoBehaviour
{
    #region Serialized Animation Settings

    [Header("Mouth Animation")]
    [SerializeField] private float mouthRotationAngle = 20f; // Maximum rotation angle for the mouth

    [Header("Cheekbones Animation")]
    [SerializeField] private float cheekbonesYOffset = -17f; // Vertical offset for cheekbones on medium/hard difficulties

    [Header("Eyes Animation")]
    [SerializeField] private Vector2 eyesEasyOffset = new Vector2(11f, 4f); // Position offset for eyes on easy difficulty

    [Header("Eyebrows Animation")]
    [SerializeField] private float eyebrowLeftEasyRotation = 20f; // Left eyebrow rotation for easy difficulty
    [SerializeField] private float eyebrowLeftMediumRotation = -5f; // Left eyebrow rotation for medium difficulty
    [SerializeField] private float eyebrowLeftHardRotation = -10f; // Left eyebrow rotation for hard difficulty

    [Header("Horns Animation")]
    [SerializeField] private float hornLeftEasyMediumRotation = 30f; // Horn rotation for easy/medium difficulties
    [SerializeField] private float hornLeftHardRotation = 0f; // Horn rotation for hard difficulty

    #endregion

    #region Serialized Icon Components

    [Header("Icon Components")]
    [SerializeField] private Image difficultyIcon; // Main icon image
    [SerializeField] private RectTransform mouth; // Mouth transform reference
    [SerializeField] private Image cheekbones; // Cheekbones image reference
    [SerializeField] private Image eyebrowLeft; // Left eyebrow image reference
    [SerializeField] private Image eyebrowRight; // Right eyebrow image reference
    [SerializeField] private RectTransform eyes; // Eyes transform reference
    [SerializeField] private RectTransform hornLeft; // Left horn transform reference
    [SerializeField] private RectTransform hornRight; // Right horn transform reference

    [Header("Dependencies")]
    [SerializeField] private DifficultyColorProvider colorProvider; // Provides colors for different difficulty levels

    #endregion

    #region Private Variables

    private RectTransform cheekbonesRect; // Cached RectTransform for cheekbones
    private Vector2 cheekbonesOriginalPosition; // Original position of cheekbones
    private Vector2 eyesOriginalPosition; // Original position of eyes

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes the animator by caching original positions of animated elements.
    /// Should be called before any animations are performed.
    /// </summary>
    public void Initialize()
    {
        // Cache cheekbones RectTransform and store original position
        if (cheekbones != null)
        {
            cheekbonesRect = cheekbones.GetComponent<RectTransform>();
            if (cheekbonesRect != null)
            {
                cheekbonesOriginalPosition = cheekbonesRect.anchoredPosition;
            }
        }

        // Store original eyes position
        if (eyes != null)
        {
            eyesOriginalPosition = eyes.anchoredPosition;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Updates all icon animations based on slider value.
    /// Coordinates all facial feature animations and color changes.
    /// </summary>
    /// <param name="sliderValue">Slider position (0 = Easy, 1 = Medium, 2 = Hard)</param>
    public void UpdateIconAnimations(float sliderValue)
    {
        // Get interpolated color based on current slider value
        Color targetColor = GetColorForSliderValue(sliderValue);

        // Update all animated components
        UpdateIconColor(targetColor);
        UpdateMouthRotation(sliderValue);
        UpdateCheekbonesPosition(sliderValue);
        UpdateEyesPosition(sliderValue);
        UpdateEyebrowsRotation(sliderValue);
        UpdateHornsRotation(sliderValue);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Gets the interpolated color for the current slider value from the color provider.
    /// </summary>
    /// <param name="sliderValue">Current slider position</param>
    /// <returns>Interpolated color for the difficulty level</returns>
    private Color GetColorForSliderValue(float sliderValue)
    {
        // Return white as fallback if color provider is missing
        if (colorProvider == null)
            return Color.white;

        return colorProvider.GetColorForSliderValue(sliderValue);
    }

    /// <summary>
    /// Updates the color of all icon components based on the target color.
    /// Applies color tint to the main icon, cheekbones, and eyebrows.
    /// </summary>
    /// <param name="targetColor">Color to apply to the icon components</param>
    private void UpdateIconColor(Color targetColor)
    {
        // Update main icon color tint
        if (difficultyIcon != null)
        {
            difficultyIcon.color = targetColor;
        }

        // Update cheekbones color
        if (cheekbones != null)
        {
            cheekbones.color = targetColor;
        }

        // Update left eyebrow color
        if (eyebrowLeft != null)
        {
            eyebrowLeft.color = targetColor;
        }

        // Update right eyebrow color
        if (eyebrowRight != null)
        {
            eyebrowRight.color = targetColor;
        }
    }

    /// <summary>
    /// Updates the mouth rotation based on slider value.
    /// Creates a smile on easy (negative rotation) and frown on hard (positive rotation).
    /// </summary>
    /// <param name="sliderValue">Current slider position</param>
    private void UpdateMouthRotation(float sliderValue)
    {
        if (mouth == null)
            return;

        // Calculate rotation based on slider position
        // Medium (1.0) = 0°
        // Easy (0.0) = -20° (clockwise, smile)
        // Hard (2.0) = +20° (counter-clockwise, frown)
        float targetRotation = (sliderValue - 1f) * mouthRotationAngle;

        // Apply rotation to mouth
        mouth.localRotation = Quaternion.Euler(0f, 0f, targetRotation);
    }

    /// <summary>
    /// Updates the cheekbones vertical position based on slider value.
    /// Moves cheekbones down on medium and hard difficulties.
    /// </summary>
    /// <param name="sliderValue">Current slider position</param>
    private void UpdateCheekbonesPosition(float sliderValue)
    {
        if (cheekbonesRect == null)
            return;

        // Calculate Y position based on slider position
        // Easy (0.0) = 0 (original position)
        // Medium (1.0) = -17 (moved down)
        // Hard (2.0) = -17 (stays down)
        float targetYOffset;

        if (sliderValue < 1f)
        {
            // Between Easy and Medium: interpolate from 0 to cheekbonesYOffset
            targetYOffset = Mathf.Lerp(0f, cheekbonesYOffset, sliderValue);
        }
        else
        {
            // Between Medium and Hard: maintain offset
            targetYOffset = cheekbonesYOffset;
        }

        // Apply vertical position offset
        cheekbonesRect.anchoredPosition = new Vector2(cheekbonesOriginalPosition.x, cheekbonesOriginalPosition.y + targetYOffset);
    }

    /// <summary>
    /// Updates the eyes position based on slider value.
    /// Eyes are offset on easy difficulty and return to original position on medium/hard.
    /// </summary>
    /// <param name="sliderValue">Current slider position</param>
    private void UpdateEyesPosition(float sliderValue)
    {
        if (eyes == null)
            return;

        // Calculate position based on slider position
        // Easy (0.0) = original position + eyesEasyOffset (11, 4)
        // Medium (1.0) = original position (0, 0)
        // Hard (2.0) = original position (0, 0)
        Vector2 targetOffset;

        if (sliderValue < 1f)
        {
            // Between Easy and Medium: interpolate from eyesEasyOffset to zero
            targetOffset = Vector2.Lerp(eyesEasyOffset, Vector2.zero, sliderValue);
        }
        else
        {
            // Between Medium and Hard: maintain original position
            targetOffset = Vector2.zero;
        }

        // Apply position offset
        eyes.anchoredPosition = eyesOriginalPosition + targetOffset;
    }

    /// <summary>
    /// Updates the eyebrows rotation based on slider value.
    /// Left eyebrow rotates progressively, right eyebrow mirrors the rotation.
    /// </summary>
    /// <param name="sliderValue">Current slider position</param>
    private void UpdateEyebrowsRotation(float sliderValue)
    {
        // Calculate rotation based on slider position
        // Easy (0.0) = eyebrowLeftEasyRotation (default +20° counter-clockwise)
        // Medium (1.0) = eyebrowLeftMediumRotation (default -5°)
        // Hard (2.0) = eyebrowLeftHardRotation (default -10° clockwise)
        float targetRotationLeft;

        if (sliderValue < 1f)
        {
            // Between Easy and Medium: interpolate rotations
            targetRotationLeft = Mathf.Lerp(eyebrowLeftEasyRotation, eyebrowLeftMediumRotation, sliderValue);
        }
        else
        {
            // Between Medium and Hard: interpolate rotations
            targetRotationLeft = Mathf.Lerp(eyebrowLeftMediumRotation, eyebrowLeftHardRotation, sliderValue - 1f);
        }

        // Apply rotation to left eyebrow
        if (eyebrowLeft != null)
        {
            RectTransform eyebrowLeftRect = eyebrowLeft.GetComponent<RectTransform>();
            if (eyebrowLeftRect != null)
            {
                eyebrowLeftRect.localRotation = Quaternion.Euler(0f, 0f, targetRotationLeft);
            }
        }

        // Apply mirrored rotation to right eyebrow
        if (eyebrowRight != null)
        {
            RectTransform eyebrowRightRect = eyebrowRight.GetComponent<RectTransform>();
            if (eyebrowRightRect != null)
            {
                // Rotate in opposite direction for symmetry
                eyebrowRightRect.localRotation = Quaternion.Euler(0f, 0f, -targetRotationLeft);
            }
        }
    }

    /// <summary>
    /// Updates the horns rotation based on slider value.
    /// Horns are rotated outward on easy/medium and straighten on hard difficulty.
    /// </summary>
    /// <param name="sliderValue">Current slider position</param>
    private void UpdateHornsRotation(float sliderValue)
    {
        // Calculate rotation based on slider position
        // Easy (0.0) = hornLeftEasyMediumRotation (default 30° outward)
        // Medium (1.0) = hornLeftEasyMediumRotation (default 30° outward)
        // Hard (2.0) = hornLeftHardRotation (default 0° straight)
        float targetRotationLeft;

        if (sliderValue < 1f)
        {
            // Between Easy and Medium: maintain rotation
            targetRotationLeft = hornLeftEasyMediumRotation;
        }
        else
        {
            // Between Medium and Hard: interpolate to straight position
            targetRotationLeft = Mathf.Lerp(hornLeftEasyMediumRotation, hornLeftHardRotation, sliderValue - 1f);
        }

        // Apply rotation to left horn
        if (hornLeft != null)
        {
            hornLeft.localRotation = Quaternion.Euler(0f, 0f, targetRotationLeft);
        }

        // Apply mirrored rotation to right horn
        if (hornRight != null)
        {
            // Rotate in opposite direction for symmetry
            hornRight.localRotation = Quaternion.Euler(0f, 0f, -targetRotationLeft);
        }
    }

    #endregion
}
