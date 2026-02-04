using UnityEngine;
using UnityEngine.UI;

public class DifficultyIconAnimator : MonoBehaviour
{
    [Header("Mouth Animation")]
    [SerializeField] private float mouthRotationAngle = 20f;

    [Header("Cheekbones Animation")]
    [SerializeField] private float cheekbonesYOffset = -17f;

    [Header("Eyes Animation")]
    [SerializeField] private Vector2 eyesEasyOffset = new Vector2(11f, 4f);

    [Header("Eyebrows Animation")]
    [SerializeField] private float eyebrowLeftEasyRotation = 20f;
    [SerializeField] private float eyebrowLeftMediumRotation = -5f;
    [SerializeField] private float eyebrowLeftHardRotation = -10f;

    [Header("Horns Animation")]
    [SerializeField] private float hornLeftEasyMediumRotation = 30f;
    [SerializeField] private float hornLeftHardRotation = 0f;

    [Header("Icon Components")]
    [SerializeField] private Image difficultyIcon;
    [SerializeField] private RectTransform mouth;
    [SerializeField] private Image cheekbones;
    [SerializeField] private Image eyebrowLeft;
    [SerializeField] private Image eyebrowRight;
    [SerializeField] private RectTransform eyes;
    [SerializeField] private RectTransform hornLeft;
    [SerializeField] private RectTransform hornRight;

    [Header("Dependencies")]
    [SerializeField] private DifficultyColorProvider colorProvider;

    private RectTransform cheekbonesRect;
    private Vector2 cheekbonesOriginalPosition;
    private Vector2 eyesOriginalPosition;

    public void Initialize()
    {
        // Store original positions
        if (cheekbones != null)
        {
            cheekbonesRect = cheekbones.GetComponent<RectTransform>();
            if (cheekbonesRect != null)
            {
                cheekbonesOriginalPosition = cheekbonesRect.anchoredPosition;
            }
        }

        if (eyes != null)
        {
            eyesOriginalPosition = eyes.anchoredPosition;
        }
    }

    /// <summary>
    /// Updates all icon animations based on slider value (0 = Easy, 1 = Medium, 2 = Hard)
    /// </summary>
    public void UpdateIconAnimations(float sliderValue)
    {
        // Get color from color provider
        Color targetColor = GetColorForSliderValue(sliderValue);

        UpdateIconColor(targetColor);
        UpdateMouthRotation(sliderValue);
        UpdateCheekbonesPosition(sliderValue);
        UpdateEyesPosition(sliderValue);
        UpdateEyebrowsRotation(sliderValue);
        UpdateHornsRotation(sliderValue);
    }

    private Color GetColorForSliderValue(float sliderValue)
    {
        if (colorProvider == null)
            return Color.white;

        return colorProvider.GetColorForSliderValue(sliderValue);
    }

    /// <summary>
    /// Updates the icon color based on the provided color
    /// </summary>
    private void UpdateIconColor(Color targetColor)
    {
        // Update DifficultyIcon color tint
        if (difficultyIcon != null)
        {
            difficultyIcon.color = targetColor;
        }

        // Update cheekbones color
        if (cheekbones != null)
        {
            cheekbones.color = targetColor;
        }

        // Update eyebrows color
        if (eyebrowLeft != null)
        {
            eyebrowLeft.color = targetColor;
        }

        if (eyebrowRight != null)
        {
            eyebrowRight.color = targetColor;
        }
    }

    private void UpdateMouthRotation(float sliderValue)
    {
        if (mouth == null)
            return;

        // Calculate rotation based on slider position
        // Medium (1.0) = 0°
        // Easy (0.0) = -20° (clockwise)
        // Hard (2.0) = +20° (counter-clockwise)
        float targetRotation = (sliderValue - 1f) * mouthRotationAngle;

        // Apply rotation
        mouth.localRotation = Quaternion.Euler(0f, 0f, targetRotation);
    }

    private void UpdateCheekbonesPosition(float sliderValue)
    {
        if (cheekbonesRect == null)
            return;

        // Calculate Y position based on slider position
        // Easy (0.0) = 0 (original position)
        // Medium (1.0) = -17
        // Hard (2.0) = -17
        float targetYOffset;

        if (sliderValue < 1f)
        {
            // Between Easy and Medium: interpolate from 0 to -17
            targetYOffset = Mathf.Lerp(0f, cheekbonesYOffset, sliderValue);
        }
        else
        {
            // Between Medium and Hard: stay at -17
            targetYOffset = cheekbonesYOffset;
        }

        // Apply position
        cheekbonesRect.anchoredPosition = new Vector2(cheekbonesOriginalPosition.x, cheekbonesOriginalPosition.y + targetYOffset);
    }

    private void UpdateEyesPosition(float sliderValue)
    {
        if (eyes == null)
            return;

        // Calculate position based on slider position
        // Easy (0.0) = original position + (11, 4)
        // Medium (1.0) = original position (0, 0)
        // Hard (2.0) = original position (0, 0)
        Vector2 targetOffset;

        if (sliderValue < 1f)
        {
            // Between Easy and Medium: interpolate from (11, 4) to (0, 0)
            targetOffset = Vector2.Lerp(eyesEasyOffset, Vector2.zero, sliderValue);
        }
        else
        {
            // Between Medium and Hard: stay at (0, 0)
            targetOffset = Vector2.zero;
        }

        // Apply position
        eyes.anchoredPosition = eyesOriginalPosition + targetOffset;
    }

    private void UpdateEyebrowsRotation(float sliderValue)
    {
        // Calculate rotation based on slider position
        // Easy (0.0) = eyebrowLeftEasyRotation (default +20° left/counter-clockwise)
        // Medium (1.0) = eyebrowLeftMediumRotation (default -5°)
        // Hard (2.0) = eyebrowLeftHardRotation (default -10° right/clockwise)
        float targetRotationLeft;

        if (sliderValue < 1f)
        {
            // Between Easy and Medium: interpolate from Easy to Medium rotation
            targetRotationLeft = Mathf.Lerp(eyebrowLeftEasyRotation, eyebrowLeftMediumRotation, sliderValue);
        }
        else
        {
            // Between Medium and Hard: interpolate from Medium to Hard rotation
            targetRotationLeft = Mathf.Lerp(eyebrowLeftMediumRotation, eyebrowLeftHardRotation, sliderValue - 1f);
        }

        // Apply rotation to EyebrowLeft
        if (eyebrowLeft != null)
        {
            RectTransform eyebrowLeftRect = eyebrowLeft.GetComponent<RectTransform>();
            if (eyebrowLeftRect != null)
            {
                eyebrowLeftRect.localRotation = Quaternion.Euler(0f, 0f, targetRotationLeft);
            }
        }

        // Apply inverse rotation to EyebrowRight
        if (eyebrowRight != null)
        {
            RectTransform eyebrowRightRect = eyebrowRight.GetComponent<RectTransform>();
            if (eyebrowRightRect != null)
            {
                // Rotate in opposite direction
                eyebrowRightRect.localRotation = Quaternion.Euler(0f, 0f, -targetRotationLeft);
            }
        }
    }

    private void UpdateHornsRotation(float sliderValue)
    {
        // Calculate rotation based on slider position
        // Easy (0.0) = hornLeftEasyMediumRotation (default 30°)
        // Medium (1.0) = hornLeftEasyMediumRotation (default 30°)
        // Hard (2.0) = hornLeftHardRotation (default 0°)
        float targetRotationLeft;

        if (sliderValue < 1f)
        {
            // Between Easy and Medium: stay at 30°
            targetRotationLeft = hornLeftEasyMediumRotation;
        }
        else
        {
            // Between Medium and Hard: interpolate from 30° to 0°
            targetRotationLeft = Mathf.Lerp(hornLeftEasyMediumRotation, hornLeftHardRotation, sliderValue - 1f);
        }

        // Apply rotation to HornLeft
        if (hornLeft != null)
        {
            hornLeft.localRotation = Quaternion.Euler(0f, 0f, targetRotationLeft);
        }

        // Apply inverse rotation to HornRight
        if (hornRight != null)
        {
            // Rotate in opposite direction (flip the image manually in the Scene)
            hornRight.localRotation = Quaternion.Euler(0f, 0f, -targetRotationLeft);
        }
    }
}
