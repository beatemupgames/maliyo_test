using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    [SerializeField] private DifficultyTextAnimator textAnimator;
    [SerializeField] private DifficultyColorProvider colorProvider;

    [Header("UI Components")]
    [SerializeField] private Button playButton;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    private Difficulty currentDifficulty = Difficulty.Easy;

    private void Start()
    {
        // Load saved difficulty
        LoadDifficulty();

        // Initialize icon animator FIRST (must store original positions before any updates)
        if (iconAnimator != null)
        {
            iconAnimator.Initialize();
        }

        // Initialize text animator
        if (textAnimator != null)
        {
            textAnimator.Initialize();
        }

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

    private void HandleSliderValueChanged(float sliderValue, Color currentColor)
    {
        // Update icon animations
        if (iconAnimator != null)
        {
            iconAnimator.UpdateIconAnimations(sliderValue, currentColor);
        }

        // Update text rotation
        if (textAnimator != null)
        {
            textAnimator.UpdateTextRotation(sliderValue);
        }

        // Update play button color
        UpdatePlayButtonColor(currentColor);
    }

    private void HandleDifficultySnapped(int difficultyValue)
    {
        currentDifficulty = (Difficulty)difficultyValue;
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
