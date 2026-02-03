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

    [Header("Controllers")]
    [SerializeField] private DifficultySliderController sliderController;
    [SerializeField] private DifficultyIconAnimator iconAnimator;

    [Header("UI Components")]
    [SerializeField] private Button playButton;
    [SerializeField] private TextMeshProUGUI difficultyText;
    [SerializeField] private TextMeshProUGUI difficultyTextNext;

    [Header("Text Animation Settings")]
    [SerializeField] private float textRotationAmount = 30f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    private Difficulty currentDifficulty = Difficulty.Easy;
    private RectTransform difficultyTextRect;
    private RectTransform difficultyTextNextRect;
    private Vector2 textOriginalPosition;
    private Vector3 textOriginalScale;

    private void Start()
    {
        // Load saved difficulty
        LoadDifficulty();

        // Initialize icon animator FIRST (must store original positions before any updates)
        if (iconAnimator != null)
        {
            iconAnimator.Initialize();
        }

        // Initialize text components
        InitializeTextComponents();

        // Initialize slider controller (this will trigger events that update iconAnimator)
        if (sliderController != null)
        {
            // Subscribe to events BEFORE initializing to catch the initial event
            sliderController.OnSliderValueChanged += HandleSliderValueChanged;
            sliderController.OnDifficultySnapped += HandleDifficultySnapped;

            // Initialize slider (this will trigger OnSliderValueChanged event)
            sliderController.Initialize((int)currentDifficulty);
        }

        // Update play button color initially
        UpdatePlayButtonColor();

        // Initialize text rotation
        UpdateTextRotation();
    }

    private void OnDestroy()
    {
        // Unsubscribe from slider events
        if (sliderController != null)
        {
            sliderController.OnSliderValueChanged -= HandleSliderValueChanged;
            sliderController.OnDifficultySnapped -= HandleDifficultySnapped;
        }
    }

    private void LoadDifficulty()
    {
        if (PlayerPrefs.HasKey("GameDifficulty"))
        {
            string savedDifficulty = PlayerPrefs.GetString("GameDifficulty");
            if (System.Enum.TryParse(savedDifficulty, true, out Difficulty loadedDifficulty))
            {
                currentDifficulty = loadedDifficulty;
            }
        }
    }

    private void InitializeTextComponents()
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

    private void HandleSliderValueChanged(float sliderValue, Color currentColor)
    {
        // Update icon animations
        if (iconAnimator != null)
        {
            iconAnimator.UpdateIconAnimations(sliderValue, currentColor);
        }

        // Update text rotation
        UpdateTextRotation();

        // Update play button color
        UpdatePlayButtonColor(currentColor);
    }

    private void HandleDifficultySnapped(int difficultyValue)
    {
        currentDifficulty = (Difficulty)difficultyValue;
    }

    private void UpdateTextRotation()
    {
        if (sliderController == null || difficultyTextRect == null)
            return;

        float sliderValue = sliderController.SliderValue;

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
        if (sliderController == null)
            return Color.white;

        // Get colors from slider controller
        Color easyColor = new Color(0.184f, 0.718f, 0.255f);
        Color mediumColor = new Color(0.992f, 0.655f, 0.016f);
        Color hardColor = new Color(0.996f, 0.373f, 0.337f);

        switch (difficultyIndex)
        {
            case 0: return easyColor;
            case 1: return mediumColor;
            case 2: return hardColor;
            default: return easyColor;
        }
    }

    private void UpdatePlayButtonColor()
    {
        if (sliderController != null)
        {
            Color currentColor = sliderController.GetCurrentDifficultyColor();
            UpdatePlayButtonColor(currentColor);
        }
    }

    private void UpdatePlayButtonColor(Color targetColor)
    {
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
