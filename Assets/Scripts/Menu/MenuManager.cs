using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    #region Enums

    public enum Difficulty
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    #endregion

    #region Serialized Fields

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("Controllers")]
    [SerializeField] private DifficultySliderController sliderController;
    [SerializeField] private DifficultyIconAnimator iconAnimator;
    [SerializeField] private DifficultyTextAnimator textAnimator;
    [SerializeField] private DifficultyColorProvider colorProvider;

    [Header("UI Components")]
    [SerializeField] private Button playButton;

    #endregion

    #region Private Fields

    private Difficulty currentDifficulty = Difficulty.Easy;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Called when the script instance is loaded.
    /// Initializes all controllers, loads saved difficulty, and sets up event listeners.
    /// </summary>
    private void Start()
    {
        // Load the previously saved difficulty preference
        LoadDifficulty();

        // Initialize icon animator first (must store original positions before any updates)
        if (iconAnimator != null)
        {
            iconAnimator.Initialize();
        }

        // Initialize text animator
        if (textAnimator != null)
        {
            textAnimator.Initialize();
        }

        // Initialize slider controller (this will trigger events that update other components)
        if (sliderController != null)
        {
            // Subscribe to events before initializing to catch the initial state change
            sliderController.OnSliderValueChanged += HandleSliderValueChanged;
            sliderController.OnDifficultySnapped += HandleDifficultySnapped;

            // Initialize slider with saved difficulty (triggers OnSliderValueChanged event)
            sliderController.Initialize((int)currentDifficulty);
        }

        // Update play button color to match initial difficulty
        UpdatePlayButtonColor();
    }

    /// <summary>
    /// Called when the MonoBehaviour is destroyed.
    /// Cleans up event subscriptions to prevent memory leaks.
    /// </summary>
    private void OnDestroy()
    {
        // Unsubscribe from slider events
        if (sliderController != null)
        {
            sliderController.OnSliderValueChanged -= HandleSliderValueChanged;
            sliderController.OnDifficultySnapped -= HandleDifficultySnapped;
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles the slider value change event.
    /// Updates icon animations, text rotation, and play button color based on the new slider value.
    /// </summary>
    /// <param name="sliderValue">The current slider value (0-2), can be fractional</param>
    /// <param name="currentColor">The color corresponding to the current slider value</param>
    private void HandleSliderValueChanged(float sliderValue, Color currentColor)
    {
        // Update icon animations based on slider position
        if (iconAnimator != null)
        {
            iconAnimator.UpdateIconAnimations(sliderValue);
        }

        // Update text rotation animation
        if (textAnimator != null)
        {
            textAnimator.UpdateTextRotation(sliderValue);
        }

        // Update play button color to match current difficulty
        UpdatePlayButtonColor(currentColor);
    }

    /// <summary>
    /// Handles the difficulty snapped event.
    /// Called when the user releases the slider and it snaps to a difficulty level.
    /// </summary>
    /// <param name="difficultyValue">The difficulty level that was snapped to (0-2)</param>
    private void HandleDifficultySnapped(int difficultyValue)
    {
        // Update current difficulty selection
        currentDifficulty = (Difficulty)difficultyValue;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Called when the Play button is clicked.
    /// Saves the selected difficulty and loads the game scene with fade transition.
    /// </summary>
    public void OnPlayButton()
    {
        // Play button click sound
        PlayClickSound();

        // Save the selected difficulty to PlayerPrefs
        PlayerPrefs.SetString("GameDifficulty", currentDifficulty.ToString());
        PlayerPrefs.Save();

        // Fade out and load the game scene
        if (SceneFadeManager.Instance != null)
        {
            SceneFadeManager.Instance.FadeOutAndLoadScene(gameSceneName);
        }
        else
        {
            // Fallback if no fade manager exists
            SceneManager.LoadScene(gameSceneName);
        }
    }

    /// <summary>
    /// Called when the Quit button is clicked.
    /// Exits the application (or stops play mode in the editor).
    /// </summary>
    public void OnQuitButton()
    {
        // Play button click sound
        PlayClickSound();

        // Quit application (different behavior in editor vs build)
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Loads the saved difficulty preference from PlayerPrefs.
    /// Defaults to Easy if no preference is saved.
    /// </summary>
    private void LoadDifficulty()
    {
        // Check if a difficulty preference exists
        if (PlayerPrefs.HasKey("GameDifficulty"))
        {
            // Retrieve and parse the saved difficulty string
            string savedDifficulty = PlayerPrefs.GetString("GameDifficulty");
            if (System.Enum.TryParse(savedDifficulty, true, out Difficulty loadedDifficulty))
            {
                currentDifficulty = loadedDifficulty;
            }
        }
    }

    /// <summary>
    /// Updates the play button color to match the current difficulty.
    /// Overload that gets the color from the slider controller.
    /// </summary>
    private void UpdatePlayButtonColor()
    {
        // Get current difficulty color from slider controller
        if (sliderController != null)
        {
            Color currentColor = sliderController.GetCurrentDifficultyColor();
            UpdatePlayButtonColor(currentColor);
        }
    }

    /// <summary>
    /// Updates the play button color with the specified color.
    /// Adjusts the color block for different button states (normal, highlighted, pressed, selected).
    /// </summary>
    /// <param name="targetColor">The color to apply to the button</param>
    private void UpdatePlayButtonColor(Color targetColor)
    {
        if (playButton != null)
        {
            // Get the current color block
            ColorBlock colors = playButton.colors;

            // Set colors for different button states
            colors.normalColor = targetColor;
            colors.highlightedColor = targetColor * 1.1f; // Slightly brighter on hover
            colors.pressedColor = targetColor * 0.8f; // Slightly darker when pressed
            colors.selectedColor = targetColor;

            // Apply the updated color block
            playButton.colors = colors;
        }
    }

    /// <summary>
    /// Plays the button click sound effect through the SoundManager.
    /// </summary>
    private void PlayClickSound()
    {
        // Play click sound if SoundManager is available
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("Click");
        }
    }

    #endregion
}
