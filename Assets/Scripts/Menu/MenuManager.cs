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
    [SerializeField] private RectTransform sliderHandle; // Reference to Handle (assign manually)
    [SerializeField] private Image handleInsideImage; // Reference to HandleInside
    [SerializeField] private Button playButton; // Reference to Play button
    [SerializeField] private DifficultyIconAnimator iconAnimator; // Reference to DifficultyIconAnimator
    [SerializeField] private Color easyDifficultyColor = new Color(0.184f, 0.718f, 0.255f); // #2FB741
    [SerializeField] private Color mediumDifficultyColor = new Color(0.992f, 0.655f, 0.016f); // #FDA704
    [SerializeField] private Color hardDifficultyColor = new Color(0.996f, 0.373f, 0.337f); // #FE5F56

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    [Header("Slider Animation")]
    [SerializeField] private float sliderSnapSpeed = 10f;
    [SerializeField] private float textRotationAmount = 30f; // Distance for full rotation between difficulties
    [SerializeField] private float handlePulseScale = 1.1f; // Scale multiplier for handle pulse animation
    [SerializeField] private float handlePulseDuration = 1.5f; // Duration of one complete pulse cycle

    private Difficulty currentDifficulty = Difficulty.Easy;
    private bool isDraggingSlider = false;
    private float targetSliderValue;
    private bool isSnapping = false;
    private RectTransform difficultyTextRect;
    private RectTransform difficultyTextNextRect;
    private Vector2 textOriginalPosition;
    private Vector3 textOriginalScale;
    private bool isSliderBeingDragged = false;
    private float handlePulseTimer = 0f;
    private Vector3 handleOriginalScale;
    private RectTransform handleInsideRect;
    private Vector3 handleInsideOriginalScale;

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

            // Store original scales for pulse animation
            if (sliderHandle != null)
            {
                handleOriginalScale = sliderHandle.localScale;
                Debug.Log($"Handle found! Original scale: {handleOriginalScale}");
            }
            else
            {
                Debug.LogWarning("Slider Handle not assigned in Inspector!");
            }

            // Get HandleInside RectTransform
            if (handleInsideImage != null)
            {
                handleInsideRect = handleInsideImage.GetComponent<RectTransform>();
                if (handleInsideRect != null)
                {
                    handleInsideOriginalScale = handleInsideRect.localScale;
                    Debug.Log($"HandleInside found! Original scale: {handleInsideOriginalScale}");
                }
            }
            else
            {
                Debug.LogWarning("HandleInside Image not assigned in Inspector!");
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

        // Update UI to reflect current difficulty
        UpdateDifficultyUI();
        UpdateHandleColor();
        UpdateTextRotation();

        // Initialize icon animations
        if (iconAnimator != null && difficultySlider != null)
        {
            iconAnimator.UpdateIconAnimations(difficultySlider.value);
        }
    }

    private void Update()
    {
        // Smoothly snap to target value when not dragging
        if (difficultySlider != null && isSnapping && !isDraggingSlider)
        {
            difficultySlider.value = Mathf.Lerp(difficultySlider.value, targetSliderValue, Time.deltaTime * sliderSnapSpeed);

            // Update handle color, text rotation, and icon animations during snapping animation
            UpdateHandleColor();
            UpdateTextRotation();
            if (iconAnimator != null)
            {
                iconAnimator.UpdateIconAnimations(difficultySlider.value);
            }

            // Stop snapping when close enough
            if (Mathf.Abs(difficultySlider.value - targetSliderValue) < 0.01f)
            {
                difficultySlider.value = targetSliderValue;
                isSnapping = false;
            }
        }

        // Animate handle pulse when user is not dragging
        if (!isSliderBeingDragged)
        {
            handlePulseTimer += Time.deltaTime;
            float normalizedTime = (handlePulseTimer % handlePulseDuration) / handlePulseDuration;

            // Create a smooth pulse using sine wave (0 -> 1 -> 0)
            float pulseValue = Mathf.Sin(normalizedTime * Mathf.PI);
            float scale = Mathf.Lerp(1f, handlePulseScale, pulseValue);

            // Animate Handle only (HandleInside will inherit the scale as a child)
            if (sliderHandle != null)
            {
                sliderHandle.localScale = handleOriginalScale * scale;
            }
        }
    }

    private void OnSliderPointerDown()
    {
        isDraggingSlider = true;
        isSnapping = false;
        isSliderBeingDragged = true;

        // Pause animation - don't reset scale, keep current size
    }

    private void OnSliderPointerUp()
    {
        isDraggingSlider = false;
        isSliderBeingDragged = false;

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
            }
        }
    }

    private void OnDifficultySliderChanged(float value)
    {
        // Update handle color, text rotation, and icon animations in real-time
        UpdateHandleColor();
        UpdateTextRotation();
        UpdateDifficultyUI();
        if (iconAnimator != null)
        {
            iconAnimator.UpdateIconAnimations(value);
        }

        // Update current difficulty when crossing thresholds
        if (isDraggingSlider)
        {
            Difficulty previewDifficulty = (Difficulty)Mathf.RoundToInt(value);
            if (previewDifficulty != currentDifficulty)
            {
                currentDifficulty = previewDifficulty;
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
