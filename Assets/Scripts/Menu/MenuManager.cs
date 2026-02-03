using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public enum Difficulty
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("Difficulty Settings")]
    [SerializeField] private Slider difficultySlider;
    [SerializeField] private TextMeshProUGUI difficultyText;
    [SerializeField] private TextMeshProUGUI difficultyTextNext; // Second text for smooth transition
    [SerializeField] private Image difficultyIcon;
    [SerializeField] private Image handleInsideImage; // Reference to HandleInside
    [SerializeField] private Button playButton; // Reference to Play button
    [SerializeField] private RectTransform mouth; // Reference to Mouth component
    [SerializeField] private Image cheekbones; // Reference to cheekbones
    [SerializeField] private Image[] eyebrows; // Reference to eyebrows
    [SerializeField] private RectTransform eyes; // Reference to Eyes component
    [SerializeField] private Sprite easyDifficultySprite;
    [SerializeField] private Sprite mediumDifficultySprite;
    [SerializeField] private Sprite hardDifficultySprite;
    [SerializeField] private Color easyDifficultyColor = new Color(0.184f, 0.718f, 0.255f); // #2FB741
    [SerializeField] private Color mediumDifficultyColor = new Color(0.992f, 0.655f, 0.016f); // #FDA704
    [SerializeField] private Color hardDifficultyColor = new Color(0.996f, 0.373f, 0.337f); // #FE5F56

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    [Header("Slider Animation")]
    [SerializeField] private float sliderSnapSpeed = 10f;
    [SerializeField] private float textRotationAmount = 30f; // Distance for full rotation between difficulties
    [SerializeField] private float mouthRotationAngle = 20f; // Rotation angle for mouth at extremes (Easy/Hard)
    [SerializeField] private float cheekbonesYOffset = -17f; // Y offset for cheekbones at Medium/Hard
    [SerializeField] private Vector2 eyesEasyOffset = new Vector2(11f, 4f); // Position offset for eyes at Easy

    private Difficulty currentDifficulty = Difficulty.Easy;
    private bool isDraggingSlider = false;
    private float targetSliderValue;
    private bool isSnapping = false;
    private RectTransform difficultyTextRect;
    private RectTransform difficultyTextNextRect;
    private Vector2 textOriginalPosition;
    private Vector3 textOriginalScale;
    private RectTransform cheekbonesRect;
    private Vector2 cheekbonesOriginalPosition;
    private Vector2 eyesOriginalPosition;

    private void Start()
    {
        // Load saved difficulty or default to Easy
        if (PlayerPrefs.HasKey("GameDifficulty"))
        {
            string savedDifficulty = PlayerPrefs.GetString("GameDifficulty");
            if (System.Enum.TryParse(savedDifficulty, true, out Difficulty loadedDifficulty))
            {
                currentDifficulty = loadedDifficulty;
            }
        }

        // Setup slider
        if (difficultySlider != null)
        {
            difficultySlider.minValue = 0;
            difficultySlider.maxValue = 2;
            difficultySlider.wholeNumbers = false; // Allow smooth movement
            difficultySlider.value = (int)currentDifficulty;
            targetSliderValue = (int)currentDifficulty;
            difficultySlider.onValueChanged.AddListener(OnDifficultySliderChanged);

            // Add event triggers for pointer down/up
            UnityEngine.EventSystems.EventTrigger trigger = difficultySlider.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (trigger == null)
            {
                trigger = difficultySlider.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }

            // Pointer down event
            UnityEngine.EventSystems.EventTrigger.Entry pointerDownEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerDownEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
            pointerDownEntry.callback.AddListener((data) => { OnSliderPointerDown(); });
            trigger.triggers.Add(pointerDownEntry);

            // Pointer up event
            UnityEngine.EventSystems.EventTrigger.Entry pointerUpEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerUpEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
            pointerUpEntry.callback.AddListener((data) => { OnSliderPointerUp(); });
            trigger.triggers.Add(pointerUpEntry);

            // Auto-find HandleInside if not assigned
            if (handleInsideImage == null)
            {
                Transform handleSlideArea = difficultySlider.transform.Find("Handle Slide Area");
                if (handleSlideArea != null)
                {
                    Transform handle = handleSlideArea.Find("Handle");
                    if (handle != null)
                    {
                        Transform handleInside = handle.Find("HandleInside");
                        if (handleInside != null)
                        {
                            handleInsideImage = handleInside.GetComponent<Image>();
                        }
                    }
                }
            }
        }

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

        // Get RectTransform of cheekbones
        if (cheekbones != null)
        {
            cheekbonesRect = cheekbones.GetComponent<RectTransform>();
            if (cheekbonesRect != null)
            {
                cheekbonesOriginalPosition = cheekbonesRect.anchoredPosition;
            }
        }

        // Get original position of eyes
        if (eyes != null)
        {
            eyesOriginalPosition = eyes.anchoredPosition;
        }

        // Update UI to reflect current difficulty
        UpdateDifficultyUI();
        UpdateHandleColor();
        UpdateTextRotation();
        UpdateMouthRotation();
        UpdateCheekbonesPosition();
        UpdateEyesPosition();
    }

    private void Update()
    {
        // Smoothly snap to target value when not dragging
        if (difficultySlider != null && isSnapping && !isDraggingSlider)
        {
            difficultySlider.value = Mathf.Lerp(difficultySlider.value, targetSliderValue, Time.deltaTime * sliderSnapSpeed);

            // Update handle color, text rotation, mouth rotation, cheekbones and eyes position during snapping animation
            UpdateHandleColor();
            UpdateTextRotation();
            UpdateMouthRotation();
            UpdateCheekbonesPosition();
            UpdateEyesPosition();

            // Stop snapping when close enough
            if (Mathf.Abs(difficultySlider.value - targetSliderValue) < 0.01f)
            {
                difficultySlider.value = targetSliderValue;
                isSnapping = false;
            }
        }
    }

    private void OnSliderPointerDown()
    {
        isDraggingSlider = true;
        isSnapping = false;
    }

    private void OnSliderPointerUp()
    {
        isDraggingSlider = false;

        // Snap to nearest value
        if (difficultySlider != null)
        {
            targetSliderValue = Mathf.Round(difficultySlider.value);
            isSnapping = true;

            // Update difficulty based on snapped value
            Difficulty newDifficulty = (Difficulty)(int)targetSliderValue;
            if (newDifficulty != currentDifficulty)
            {
                currentDifficulty = newDifficulty;
                UpdateDifficultyUI();
                PlayClickSound();
            }
        }
    }

    private void OnDifficultySliderChanged(float value)
    {
        // Update handle color, text rotation, icon, mouth rotation, cheekbones and eyes position in real-time
        UpdateHandleColor();
        UpdateTextRotation();
        UpdateDifficultyUI();
        UpdateMouthRotation();
        UpdateCheekbonesPosition();
        UpdateEyesPosition();

        // Play sound when crossing difficulty thresholds
        if (isDraggingSlider)
        {
            Difficulty previewDifficulty = (Difficulty)Mathf.RoundToInt(value);
            if (previewDifficulty != currentDifficulty)
            {
                currentDifficulty = previewDifficulty;
                PlayClickSound();
            }
        }
    }

    private void UpdateDifficultyUI()
    {
        // Icon color is now updated in UpdateHandleColor() method
        // This method is kept for potential future UI updates
    }

    private void UpdateTextRotation()
    {
        if (difficultySlider == null || difficultyTextRect == null)
            return;

        float sliderValue = difficultySlider.value;

        // Determine which two difficulties we're between
        int lowerDifficulty = Mathf.FloorToInt(sliderValue);
        int upperDifficulty = Mathf.CeilToInt(sliderValue);
        float t = sliderValue - lowerDifficulty; // 0 to 1 between two difficulties

        // Get difficulty names and colors
        string lowerName = GetDifficultyName(lowerDifficulty);
        string upperName = GetDifficultyName(upperDifficulty);
        Color lowerColor = GetDifficultyColor(lowerDifficulty);
        Color upperColor = GetDifficultyColor(upperDifficulty);

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

    private Color GetDifficultyColor(int difficultyIndex)
    {
        switch (difficultyIndex)
        {
            case 0: return easyDifficultyColor;
            case 1: return mediumDifficultyColor;
            case 2: return hardDifficultyColor;
            default: return easyDifficultyColor;
        }
    }

    private void UpdateHandleColor()
    {
        if (difficultySlider == null)
            return;

        float sliderValue = difficultySlider.value;
        Color targetColor;

        // Interpolate color based on slider position
        if (sliderValue <= 1f)
        {
            // Between Easy (0) and Medium (1) - interpolate between green and orange
            float t = sliderValue; // 0 to 1
            targetColor = Color.Lerp(easyDifficultyColor, mediumDifficultyColor, t);
        }
        else
        {
            // Between Medium (1) and Hard (2) - interpolate between orange and red
            float t = sliderValue - 1f; // 0 to 1
            targetColor = Color.Lerp(mediumDifficultyColor, hardDifficultyColor, t);
        }

        // Update HandleInside color
        if (handleInsideImage != null)
        {
            handleInsideImage.color = targetColor;
        }

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
        if (eyebrows != null)
        {
            foreach (Image eyebrow in eyebrows)
            {
                if (eyebrow != null)
                {
                    eyebrow.color = targetColor;
                }
            }
        }

        // Update Play button color
        if (playButton != null)
        {
            ColorBlock colors = playButton.colors;
            colors.normalColor = targetColor;
            colors.highlightedColor = targetColor * 1.1f; // Slightly brighter on hover
            colors.pressedColor = targetColor * 0.8f; // Slightly darker when pressed
            colors.selectedColor = targetColor;
            playButton.colors = colors;
        }
    }

    private void UpdateMouthRotation()
    {
        if (mouth == null || difficultySlider == null)
            return;

        float sliderValue = difficultySlider.value;

        // Calculate rotation based on slider position
        // Medium (1.0) = 0°
        // Easy (0.0) = -20° (clockwise)
        // Hard (2.0) = +20° (counter-clockwise)
        float targetRotation = (sliderValue - 1f) * mouthRotationAngle;

        // Apply rotation
        mouth.localRotation = Quaternion.Euler(0f, 0f, targetRotation);
    }

    private void UpdateCheekbonesPosition()
    {
        if (cheekbonesRect == null || difficultySlider == null)
            return;

        float sliderValue = difficultySlider.value;

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

    private void UpdateEyesPosition()
    {
        if (eyes == null || difficultySlider == null)
            return;

        float sliderValue = difficultySlider.value;

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

    public void OnPlayButton()
    {
        PlayClickSound();

        // Save the selected difficulty
        PlayerPrefs.SetString("GameDifficulty", currentDifficulty.ToString());
        PlayerPrefs.Save();

        // Load the game scene
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnQuitButton()
    {
        PlayClickSound();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
