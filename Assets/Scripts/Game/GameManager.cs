using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;

public class GameManager : MonoBehaviour
{
    #region Enums

    public enum ButtonColor { Blue, Green, Yellow, Red }

    public enum GameState
    {
        Idle,
        ShowingSequence,
        WaitingForPlayerInput,
        GameOver
    }

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    #endregion

    #region Nested Classes

    /// <summary>
    /// Represents a single step in the game sequence.
    /// Can contain one or more buttons that need to be pressed simultaneously.
    /// </summary>
    [System.Serializable]
    public class SequenceStep
    {
        public List<ButtonColor> buttons = new List<ButtonColor>();

        public SequenceStep(ButtonColor button)
        {
            buttons.Add(button);
        }

        public SequenceStep(List<ButtonColor> buttonList)
        {
            buttons = new List<ButtonColor>(buttonList);
        }
    }

    /// <summary>
    /// Represents a single score entry with its date.
    /// Used for tracking high scores over time.
    /// </summary>
    [System.Serializable]
    public class ScoreEntry
    {
        public int score;
        public string date;

        public ScoreEntry(int score, string date)
        {
            this.score = score;
            this.date = date;
        }
    }

    /// <summary>
    /// Manages scores for a specific difficulty level.
    /// Provides methods to retrieve best scores for different time periods.
    /// </summary>
    [System.Serializable]
    public class DifficultyScores
    {
        public List<ScoreEntry> scores = new List<ScoreEntry>();

        /// <summary>
        /// Gets the highest score ever achieved for this difficulty.
        /// </summary>
        public int GetBestScoreAllTime()
        {
            int best = 0;
            foreach (ScoreEntry entry in scores)
            {
                if (entry.score > best)
                {
                    best = entry.score;
                }
            }
            return best;
        }

        /// <summary>
        /// Gets the highest score achieved today for this difficulty.
        /// </summary>
        public int GetBestScoreToday()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            int best = 0;
            foreach (ScoreEntry entry in scores)
            {
                if (entry.date == today && entry.score > best)
                {
                    best = entry.score;
                }
            }
            return best;
        }

        /// <summary>
        /// Gets the highest score achieved this week for this difficulty.
        /// Week starts on Sunday.
        /// </summary>
        public int GetBestScoreThisWeek()
        {
            DateTime now = DateTime.Now;
            DateTime weekStart = now.AddDays(-(int)now.DayOfWeek);
            string weekStartStr = weekStart.ToString("yyyy-MM-dd");

            int best = 0;
            foreach (ScoreEntry entry in scores)
            {
                DateTime entryDate;
                if (DateTime.TryParse(entry.date, out entryDate))
                {
                    if (entryDate >= weekStart && entry.score > best)
                    {
                        best = entry.score;
                    }
                }
            }
            return best;
        }

        /// <summary>
        /// Adds a new score entry with today's date.
        /// </summary>
        public void AddScore(int score)
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            scores.Add(new ScoreEntry(score, today));
        }
    }

    /// <summary>
    /// Container for all high score data across all difficulty levels.
    /// Used for JSON serialization/deserialization.
    /// </summary>
    [System.Serializable]
    public class HighScoreData
    {
        public DifficultyScores easyScores = new DifficultyScores();
        public DifficultyScores mediumScores = new DifficultyScores();
        public DifficultyScores hardScores = new DifficultyScores();
    }

    #endregion

    #region Serialized Fields

    [Header("General Settings")]
    [SerializeField] private Difficulty difficulty = Difficulty.Medium;
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField] private bool enableDebugLogs = false;

    [Header("Sequence & Button Timing")]
    [SerializeField] private float timeBetweenButtons = 0.6f;
    [SerializeField] private float buttonPressDuration = 0.4f;
    [SerializeField] private float simultaneousButtonDelay = 0.05f;

    [Header("Hard Mode")]
    [SerializeField] private float hardModeRotationSpeed = 30f;

    [Header("Score Animation")]
    [SerializeField] private Color scoreHighlightColor = Color.yellow;
    [SerializeField] private float scoreAnimationDuration = 0.3f;
    [SerializeField] private float scoreScaleMultiplier = 1.3f;

    [Header("Game Over Score Animation")]
    [SerializeField] private Color scoreGameOverColor = Color.red;
    [SerializeField] private float scoreGameOverTransitionDuration = 0.2f;
    [SerializeField] private float scoreGameOverHoldDuration = 0.6f;

    [Header("Game State")]
    [SerializeField] private GameState currentState = GameState.Idle;
    [SerializeField] private List<SequenceStep> sequence = new List<SequenceStep>();
    [SerializeField] private int currentPlayerStep = 0;
    [SerializeField] private int currentRound = 0;
    [SerializeField] private int playerScore = 0;

    [Header("Button References")]
    [SerializeField] private SimonButton[] easyButtons = new SimonButton[3];
    [SerializeField] private SimonButton[] normalButtons = new SimonButton[4];

    [Header("UI References")]
    [SerializeField] private TextMeshPro scoreNumberText2D;
    [SerializeField] private TextMeshPro scoreText2D;
    [SerializeField] private GameHUDManager gameHUDManager;
    [SerializeField] private PanelGameOverManager panelGameOverManager;
    [SerializeField] private PanelScoreManager panelScoreManager;

    [Header("Game Elements")]
    [SerializeField] private GameObject simonEasyGameObject;
    [SerializeField] private GameObject simonNormalGameObject;

    #endregion

    #region Private Fields

    private HashSet<ButtonColor> currentStepPressedButtons = new HashSet<ButtonColor>();
    private HighScoreData highScoreData = new HighScoreData();
    private string saveFilePath;
    private Color scoreNumberText2DOriginalColor;
    private Vector3 scoreNumberText2DOriginalScale;
    private Color scoreText2DOriginalColor;

    #endregion

    #region Properties

    public GameState CurrentState => currentState;
    public int CurrentRound => currentRound;
    public int PlayerScore => playerScore;

    private SimonButton[] CurrentButtons => difficulty == Difficulty.Easy ? easyButtons : normalButtons;
    private GameObject CurrentSimonGameObject => difficulty == Difficulty.Easy ? simonEasyGameObject : simonNormalGameObject;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Called once per frame.
    /// Handles continuous rotation of Simon GameObject in Hard mode.
    /// </summary>
    private void Update()
    {
        // Rotate Simon GameObject continuously in Hard mode (except during Game Over)
        if (difficulty == Difficulty.Hard && simonNormalGameObject != null && currentState != GameState.GameOver)
        {
            simonNormalGameObject.transform.Rotate(0f, 0f, hardModeRotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Called when the script instance is loaded.
    /// Initializes game settings, loads high scores, and starts a new game.
    /// </summary>
    private void Start()
    {
        // Load difficulty preference from PlayerPrefs (set in menu)
        if (PlayerPrefs.HasKey("GameDifficulty"))
        {
            string difficultyString = PlayerPrefs.GetString("GameDifficulty");
            if (System.Enum.TryParse(difficultyString, true, out Difficulty loadedDifficulty))
            {
                difficulty = loadedDifficulty;
            }
        }

        // Store original score text colors and scale for animations
        if (scoreNumberText2D != null)
        {
            scoreNumberText2DOriginalColor = scoreNumberText2D.color;
            scoreNumberText2DOriginalScale = scoreNumberText2D.transform.localScale;
        }

        if (scoreText2D != null)
        {
            scoreText2DOriginalColor = scoreText2D.color;
        }

        // Setup save file path and load high scores from disk
        saveFilePath = Path.Combine(Application.persistentDataPath, "highscores.json");
        LoadHighScores();

        // Initialize UI managers with current difficulty and best scores
        if (gameHUDManager != null)
        {
            gameHUDManager.Initialize(difficulty, GetBestScoreAllTime());
        }

        if (panelScoreManager != null)
        {
            panelScoreManager.UpdateDifficultyUI(difficulty);
        }

        // Activate appropriate Simon GameObject based on difficulty
        if (simonEasyGameObject != null)
        {
            simonEasyGameObject.SetActive(difficulty == Difficulty.Easy);
            simonEasyGameObject.transform.rotation = Quaternion.identity;
        }

        if (simonNormalGameObject != null)
        {
            simonNormalGameObject.SetActive(difficulty != Difficulty.Easy);
            simonNormalGameObject.transform.rotation = Quaternion.identity;
        }

        // Hide UI panels initially
        if (panelGameOverManager != null)
        {
            panelGameOverManager.gameObject.SetActive(false);
        }

        if (panelScoreManager != null)
        {
            panelScoreManager.gameObject.SetActive(false);
        }

        // Start the first game
        StartNewGame();
    }

    #endregion

    #region Game Flow

    /// <summary>
    /// Starts a new game, resetting all game state and UI.
    /// Called at start and when player chooses to play again.
    /// </summary>
    public void StartNewGame()
    {
        // Reset game state
        sequence.Clear();
        currentPlayerStep = 0;
        currentRound = 0;
        playerScore = 0;
        currentState = GameState.Idle;

        // Hide UI panels
        if (panelGameOverManager != null)
        {
            panelGameOverManager.gameObject.SetActive(false);
        }

        if (panelScoreManager != null)
        {
            panelScoreManager.ResetPanel();
            panelScoreManager.gameObject.SetActive(false);
        }

        // Activate appropriate Simon GameObject and reset rotation
        if (simonEasyGameObject != null)
        {
            simonEasyGameObject.SetActive(difficulty == Difficulty.Easy);
            simonEasyGameObject.transform.rotation = Quaternion.identity;
        }

        if (simonNormalGameObject != null)
        {
            simonNormalGameObject.SetActive(difficulty != Difficulty.Easy);
            simonNormalGameObject.transform.rotation = Quaternion.identity;
        }

        // Restore visual elements and start first round
        RestoreGameElementsAlpha();
        UpdateScoreUI(false);
        StartNextRound();
    }

    /// <summary>
    /// Starts the next round by incrementing the round counter and adding a new button to the sequence.
    /// Begins showing the sequence to the player.
    /// </summary>
    private void StartNextRound()
    {
        // Increment round counter
        currentRound++;
        currentPlayerStep = 0;

        // Add a new step to the sequence
        AddNewButtonToSequence();

        // Show the updated sequence to the player
        StartCoroutine(ShowSequenceCoroutine());
    }

    #endregion

    #region Sequence Generation

    /// <summary>
    /// Adds a new button step to the sequence based on current difficulty.
    /// Easy: Uses only 3 buttons (Blue, Green, Red).
    /// Medium: Uses all 4 buttons, one at a time.
    /// Hard: Uses all 4 buttons, with 30% chance of two simultaneous buttons.
    /// </summary>
    private void AddNewButtonToSequence()
    {
        SequenceStep newStep;

        switch (difficulty)
        {
            case Difficulty.Easy:
                // Easy mode uses only 3 buttons
                ButtonColor easyButton = GetRandomButtonForEasy();
                newStep = new SequenceStep(easyButton);
                break;

            case Difficulty.Medium:
                // Medium mode uses all 4 buttons, one at a time
                ButtonColor mediumButton = (ButtonColor)UnityEngine.Random.Range(0, 4);
                newStep = new SequenceStep(mediumButton);
                break;

            case Difficulty.Hard:
                // Hard mode can have simultaneous button presses (30% chance)
                if (UnityEngine.Random.value < 0.3f)
                {
                    List<ButtonColor> doubleButtons = new List<ButtonColor>();
                    ButtonColor first = (ButtonColor)UnityEngine.Random.Range(0, 4);
                    ButtonColor second;

                    // Ensure second button is different from first
                    do
                    {
                        second = (ButtonColor)UnityEngine.Random.Range(0, 4);
                    } while (second == first);

                    doubleButtons.Add(first);
                    doubleButtons.Add(second);
                    newStep = new SequenceStep(doubleButtons);
                }
                else
                {
                    // Single button press
                    ButtonColor hardButton = (ButtonColor)UnityEngine.Random.Range(0, 4);
                    newStep = new SequenceStep(hardButton);
                }
                break;

            default:
                // Fallback to medium behavior
                ButtonColor defaultButton = (ButtonColor)UnityEngine.Random.Range(0, 4);
                newStep = new SequenceStep(defaultButton);
                break;
        }

        // Add the new step to the sequence
        sequence.Add(newStep);
    }

    /// <summary>
    /// Gets a random button color for Easy mode (only Blue, Green, or Red).
    /// Yellow is excluded in Easy mode.
    /// </summary>
    /// <returns>A random ButtonColor from the easy mode set</returns>
    private ButtonColor GetRandomButtonForEasy()
    {
        int randomIndex = UnityEngine.Random.Range(0, 3);
        switch (randomIndex)
        {
            case 0: return ButtonColor.Blue;
            case 1: return ButtonColor.Green;
            case 2: return ButtonColor.Red;
            default: return ButtonColor.Blue;
        }
    }

    #endregion

    #region Sequence Display

    /// <summary>
    /// Coroutine that displays the entire sequence to the player.
    /// Handles both single button presses and simultaneous button presses.
    /// </summary>
    private IEnumerator ShowSequenceCoroutine()
    {
        // Change state and disable buttons during sequence display
        currentState = GameState.ShowingSequence;
        SetButtonsInteractable(false);

        // Brief pause before starting the sequence
        yield return new WaitForSeconds(0.5f);

        // Display each step in the sequence
        foreach (SequenceStep step in sequence)
        {
            if (step.buttons.Count == 1)
            {
                // Single button press
                ActivateButton(step.buttons[0]);
                yield return new WaitForSeconds(buttonPressDuration);
                DeactivateButton(step.buttons[0]);
            }
            else if (step.buttons.Count > 1)
            {
                // Multiple simultaneous buttons (Hard mode)
                // Activate all buttons with slight delay between each
                foreach (ButtonColor button in step.buttons)
                {
                    ActivateButton(button);
                    yield return new WaitForSeconds(simultaneousButtonDelay);
                }

                // Hold all buttons active
                yield return new WaitForSeconds(buttonPressDuration - (simultaneousButtonDelay * step.buttons.Count));

                // Deactivate all buttons
                foreach (ButtonColor button in step.buttons)
                {
                    DeactivateButton(button);
                }
            }

            // Pause between sequence steps
            yield return new WaitForSeconds(timeBetweenButtons - buttonPressDuration);
        }

        // Sequence complete, now wait for player input
        currentState = GameState.WaitingForPlayerInput;
        currentStepPressedButtons.Clear();
        SetButtonsInteractable(true);
    }

    #endregion

    #region Player Input Handling

    /// <summary>
    /// Called when the player presses a button.
    /// Validates the input and handles the result (correct or incorrect).
    /// </summary>
    /// <param name="buttonPressed">The color of the button that was pressed</param>
    public void OnPlayerPressButton(ButtonColor buttonPressed)
    {
        // Only accept input when waiting for player
        if (currentState != GameState.WaitingForPlayerInput)
        {
            return;
        }

        // Validate input immediately to prevent multiple simultaneous inputs
        bool isCorrect = CheckPlayerInput(buttonPressed);

        if (!isCorrect)
        {
            // Set GameOver state immediately to block further clicks
            currentState = GameState.GameOver;
        }

        // Disable buttons immediately to prevent multiple inputs during animation
        SetButtonsInteractable(false);
        StartCoroutine(HandlePlayerInputCoroutine(buttonPressed, isCorrect));
    }

    /// <summary>
    /// Coroutine that handles player input animation and result processing.
    /// </summary>
    /// <param name="buttonPressed">The button that was pressed</param>
    /// <param name="isCorrect">Whether the input was correct</param>
    private IEnumerator HandlePlayerInputCoroutine(ButtonColor buttonPressed, bool isCorrect)
    {
        // Show button press animation
        ActivateButton(buttonPressed);
        yield return new WaitForSeconds(buttonPressDuration);
        DeactivateButton(buttonPressed);

        // Handle result
        if (isCorrect)
        {
            OnPlayerCorrect();
        }
        else
        {
            OnPlayerFail();
        }
    }

    /// <summary>
    /// Checks if the player's button press is correct for the current step.
    /// Handles both single button steps and multi-button steps.
    /// </summary>
    /// <param name="buttonPressed">The button that was pressed</param>
    /// <returns>True if the input is correct, false otherwise</returns>
    private bool CheckPlayerInput(ButtonColor buttonPressed)
    {
        SequenceStep currentStep = sequence[currentPlayerStep];

        if (currentStep.buttons.Count == 1)
        {
            // Single button step: must match exactly
            return buttonPressed == currentStep.buttons[0];
        }
        else
        {
            // Multi-button step (Hard mode)
            // Check if button was already pressed in this step
            if (currentStepPressedButtons.Contains(buttonPressed))
            {
                return false;
            }

            // Check if button is part of the required buttons
            if (!currentStep.buttons.Contains(buttonPressed))
            {
                return false;
            }

            // Valid button for this multi-button step
            currentStepPressedButtons.Add(buttonPressed);
            return true;
        }
    }

    /// <summary>
    /// Handles a correct player input.
    /// Advances to next step or starts next round if sequence is complete.
    /// </summary>
    private void OnPlayerCorrect()
    {
        SequenceStep currentStep = sequence[currentPlayerStep];

        // For multi-button steps, wait until all buttons are pressed
        if (currentStep.buttons.Count > 1 && currentStepPressedButtons.Count < currentStep.buttons.Count)
        {
            // Re-enable buttons for the next button in the multi-button step
            SetButtonsInteractable(true);
            return;
        }

        // Move to next step
        currentPlayerStep++;
        currentStepPressedButtons.Clear();

        // Check if sequence is complete
        if (currentPlayerStep >= sequence.Count)
        {
            // Player completed the round
            playerScore++;
            UpdateScoreUI();
            StartNextRound();
        }
        else
        {
            // Continue with next step in the sequence
            SetButtonsInteractable(true);
        }
    }

    /// <summary>
    /// Handles an incorrect player input (game over).
    /// Saves the high score and starts the game over sequence.
    /// </summary>
    private void OnPlayerFail()
    {
        // State is already set to GameOver in OnPlayerPressButton
        // Ensure buttons stay disabled
        SetButtonsInteractable(false);

        // Save the score and show game over sequence
        SaveHighScore();
        StartCoroutine(GameOverSequenceCoroutine());

        if (enableDebugLogs)
        {
            Debug.Log($"Game Over! Final Score: {playerScore}, Rounds Completed: {currentRound - 1}");
        }
    }

    #endregion

    #region Game Over Handling

    /// <summary>
    /// Coroutine that plays the game over sequence.
    /// Flashes all buttons, animates score text, and shows the game over panel.
    /// </summary>
    private IEnumerator GameOverSequenceCoroutine()
    {
        // Play game over sound effect
        if (panelGameOverManager != null)
        {
            panelGameOverManager.PlayGameOverSound();
        }

        // Start score text red flash animation
        StartCoroutine(ScoreTextRedFlashCoroutine());

        // Flash all buttons twice to indicate game over
        for (int i = 0; i < 2; i++)
        {
            // Activate all buttons in game over mode
            foreach (SimonButton button in CurrentButtons)
            {
                if (button != null)
                {
                    button.ActivateGameOver();
                }
            }

            yield return new WaitForSeconds(0.3f);

            // Deactivate all buttons
            foreach (SimonButton button in CurrentButtons)
            {
                if (button != null)
                {
                    button.Deactivate();
                }
            }

            yield return new WaitForSeconds(0.3f);
        }

        // Show game over panel with fade in animation
        if (panelGameOverManager != null)
        {
            panelGameOverManager.ShowPanel();
            yield return StartCoroutine(panelGameOverManager.FadeInCoroutine());
        }
    }

    #endregion

    #region Button Management

    /// <summary>
    /// Activates (lights up) the button with the specified color.
    /// </summary>
    /// <param name="buttonColor">The color of the button to activate</param>
    private void ActivateButton(ButtonColor buttonColor)
    {
        SimonButton button = GetButtonByColor(buttonColor);
        if (button != null)
        {
            button.Activate();
        }
    }

    /// <summary>
    /// Deactivates (turns off) the button with the specified color.
    /// </summary>
    /// <param name="buttonColor">The color of the button to deactivate</param>
    private void DeactivateButton(ButtonColor buttonColor)
    {
        SimonButton button = GetButtonByColor(buttonColor);
        if (button != null)
        {
            button.Deactivate();
        }
    }

    /// <summary>
    /// Gets the SimonButton component with the specified color from the current button set.
    /// </summary>
    /// <param name="color">The color to search for</param>
    /// <returns>The matching SimonButton, or null if not found</returns>
    private SimonButton GetButtonByColor(ButtonColor color)
    {
        foreach (SimonButton button in CurrentButtons)
        {
            if (button != null && button.ButtonColor == color)
            {
                return button;
            }
        }
        return null;
    }

    /// <summary>
    /// Sets whether all buttons in the current set are interactable (clickable).
    /// </summary>
    /// <param name="interactable">True to enable button interaction, false to disable</param>
    private void SetButtonsInteractable(bool interactable)
    {
        foreach (SimonButton button in CurrentButtons)
        {
            if (button != null)
            {
                button.SetInteractable(interactable);
            }
        }
    }

    #endregion

    #region Public Button Callbacks

    /// <summary>
    /// Called when the player clicks the Restart button.
    /// Only works if the game is in Game Over state.
    /// </summary>
    public void RestartGame()
    {
        if (currentState == GameState.GameOver)
        {
            StartNewGame();
        }
    }

    /// <summary>
    /// Called when the player clicks the "No Thanks" button on the game over panel.
    /// Transitions to the score summary panel.
    /// </summary>
    public void OnNoThanksButtonPressed()
    {
        if (currentState == GameState.GameOver)
        {
            StartCoroutine(NoThanksSequenceCoroutine());
        }
    }

    /// <summary>
    /// Coroutine that handles the transition from game over panel to score panel.
    /// Fades out game elements and displays final scores.
    /// </summary>
    private IEnumerator NoThanksSequenceCoroutine()
    {
        // Fade out and hide game over panel
        if (panelGameOverManager != null)
        {
            yield return StartCoroutine(panelGameOverManager.FadeOutCoroutine());
            panelGameOverManager.HidePanel();
        }

        // Show score panel with all score statistics
        if (panelScoreManager != null)
        {
            yield return new WaitForSeconds(panelScoreManager.ScoreWaitDuration);

            // Activate panel
            panelScoreManager.ShowPanel();

            // Calculate best scores including current score
            int bestToday = Mathf.Max(GetBestScoreToday(), playerScore);
            int bestWeek = Mathf.Max(GetBestScoreThisWeek(), playerScore);
            int bestAllTime = Mathf.Max(GetBestScoreAllTime(), playerScore);

            // Update panel UI with scores
            panelScoreManager.UpdatePanelScoreUI(playerScore, bestToday, bestWeek, bestAllTime, difficulty);
            panelScoreManager.PlayPanelScoreSound();
            panelScoreManager.ActivateTriangles();

            // Fade out game elements while fading in score panel
            StartCoroutine(FadeOutGameElementsCoroutine(panelScoreManager.ScoreFadeInDuration));
            panelScoreManager.StartTriangleAnimations();
            yield return StartCoroutine(panelScoreManager.FadeInPanelCoroutine(panelScoreManager.ScoreFadeInDuration));
        }
    }

    #endregion

    #region Visual Effects

    /// <summary>
    /// Coroutine that fades out all game elements (Simon, buttons, score text).
    /// Used when transitioning to the score panel.
    /// </summary>
    /// <param name="duration">Duration of the fade effect in seconds</param>
    private IEnumerator FadeOutGameElementsCoroutine(float duration)
    {
        // Collect all sprite renderers and text components from Simon GameObject
        SpriteRenderer[] simonSpriteRenderers = null;
        Color[] spriteOriginalColors = null;
        TextMeshPro[] simonTextMeshPros = null;
        Color[] textOriginalColors = null;

        GameObject currentSimon = CurrentSimonGameObject;

        if (currentSimon != null)
        {
            // Get all sprite renderers and store their original colors
            simonSpriteRenderers = currentSimon.GetComponentsInChildren<SpriteRenderer>();
            if (simonSpriteRenderers != null && simonSpriteRenderers.Length > 0)
            {
                spriteOriginalColors = new Color[simonSpriteRenderers.Length];
                for (int i = 0; i < simonSpriteRenderers.Length; i++)
                {
                    spriteOriginalColors[i] = simonSpriteRenderers[i].color;
                }
            }

            // Get all TextMeshPro components and store their original colors
            simonTextMeshPros = currentSimon.GetComponentsInChildren<TextMeshPro>();
            if (simonTextMeshPros != null && simonTextMeshPros.Length > 0)
            {
                textOriginalColors = new Color[simonTextMeshPros.Length];
                for (int i = 0; i < simonTextMeshPros.Length; i++)
                {
                    textOriginalColors[i] = simonTextMeshPros[i].color;
                }
            }
        }

        float elapsedTime = 0f;

        // Animate alpha from 1 to 0 over duration
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);

            // Fade sprite renderers
            if (simonSpriteRenderers != null && spriteOriginalColors != null)
            {
                for (int i = 0; i < simonSpriteRenderers.Length; i++)
                {
                    if (simonSpriteRenderers[i] != null)
                    {
                        Color color = spriteOriginalColors[i];
                        color.a = alpha;
                        simonSpriteRenderers[i].color = color;
                    }
                }
            }

            // Fade text components
            if (simonTextMeshPros != null && textOriginalColors != null)
            {
                for (int i = 0; i < simonTextMeshPros.Length; i++)
                {
                    if (simonTextMeshPros[i] != null)
                    {
                        Color color = textOriginalColors[i];
                        color.a = alpha;
                        simonTextMeshPros[i].color = color;
                    }
                }
            }

            // Fade out score text elements
            if (scoreText2D != null)
            {
                Color scoreTextColor = scoreText2DOriginalColor;
                scoreTextColor.a = alpha;
                scoreText2D.color = scoreTextColor;
            }

            if (scoreNumberText2D != null)
            {
                Color scoreNumberColor = scoreNumberText2DOriginalColor;
                scoreNumberColor.a = alpha;
                scoreNumberText2D.color = scoreNumberColor;
            }

            yield return null;
        }

        // Ensure all elements are fully transparent at the end
        if (simonSpriteRenderers != null && spriteOriginalColors != null)
        {
            for (int i = 0; i < simonSpriteRenderers.Length; i++)
            {
                if (simonSpriteRenderers[i] != null)
                {
                    Color color = spriteOriginalColors[i];
                    color.a = 0f;
                    simonSpriteRenderers[i].color = color;
                }
            }
        }

        if (simonTextMeshPros != null && textOriginalColors != null)
        {
            for (int i = 0; i < simonTextMeshPros.Length; i++)
            {
                if (simonTextMeshPros[i] != null)
                {
                    Color color = textOriginalColors[i];
                    color.a = 0f;
                    simonTextMeshPros[i].color = color;
                }
            }
        }

        // Ensure score texts are fully transparent
        if (scoreText2D != null)
        {
            Color scoreTextColor = scoreText2DOriginalColor;
            scoreTextColor.a = 0f;
            scoreText2D.color = scoreTextColor;
        }

        if (scoreNumberText2D != null)
        {
            Color scoreNumberColor = scoreNumberText2DOriginalColor;
            scoreNumberColor.a = 0f;
            scoreNumberText2D.color = scoreNumberColor;
        }
    }

    /// <summary>
    /// Restores the alpha of all game elements to full opacity.
    /// Called when starting a new game.
    /// </summary>
    private void RestoreGameElementsAlpha()
    {
        GameObject currentSimon = CurrentSimonGameObject;

        if (currentSimon != null)
        {
            // Restore sprite renderers alpha
            SpriteRenderer[] spriteRenderers = currentSimon.GetComponentsInChildren<SpriteRenderer>();
            if (spriteRenderers != null)
            {
                foreach (SpriteRenderer spriteRenderer in spriteRenderers)
                {
                    if (spriteRenderer != null)
                    {
                        Color color = spriteRenderer.color;
                        color.a = 1f;
                        spriteRenderer.color = color;
                    }
                }
            }

            // Restore text components alpha
            TextMeshPro[] textMeshPros = currentSimon.GetComponentsInChildren<TextMeshPro>();
            if (textMeshPros != null)
            {
                foreach (TextMeshPro textMeshPro in textMeshPros)
                {
                    if (textMeshPro != null)
                    {
                        Color color = textMeshPro.color;
                        color.a = 1f;
                        textMeshPro.color = color;
                    }
                }
            }
        }

        // Restore score texts alpha
        if (scoreText2D != null)
        {
            Color scoreTextColor = scoreText2DOriginalColor;
            scoreTextColor.a = 1f;
            scoreText2D.color = scoreTextColor;
        }

        if (scoreNumberText2D != null)
        {
            Color scoreNumberColor = scoreNumberText2DOriginalColor;
            scoreNumberColor.a = 1f;
            scoreNumberText2D.color = scoreNumberColor;
        }
    }

    #endregion

    #region Score UI Animation

    /// <summary>
    /// Updates the score UI elements with the current player score.
    /// </summary>
    /// <param name="animate">Whether to play the score increase animation</param>
    private void UpdateScoreUI(bool animate = true)
    {
        string scoreString = playerScore.ToString();

        // Update 2D score text
        if (scoreNumberText2D != null)
        {
            scoreNumberText2D.text = scoreString;
            if (animate && playerScore > 0)
            {
                StartCoroutine(AnimateScoreTextCoroutine());
            }
        }

        // Update HUD with current and best scores
        if (gameHUDManager != null)
        {
            int bestScore = Mathf.Max(GetBestScoreAllTime(), playerScore);
            gameHUDManager.UpdateScores(playerScore, bestScore);
        }
    }

    /// <summary>
    /// Coroutine that animates the score text when the player completes a round.
    /// Scales up and changes color, then returns to normal.
    /// </summary>
    private IEnumerator AnimateScoreTextCoroutine()
    {
        if (scoreNumberText2D == null) yield break;

        Transform textTransform = scoreNumberText2D.transform;
        float elapsedTime = 0f;
        float halfDuration = scoreAnimationDuration / 2f;

        // Phase 1: Scale up and change color to highlight
        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / halfDuration;

            textTransform.localScale = Vector3.Lerp(scoreNumberText2DOriginalScale, scoreNumberText2DOriginalScale * scoreScaleMultiplier, t);
            scoreNumberText2D.color = Color.Lerp(scoreNumberText2DOriginalColor, scoreHighlightColor, t);

            yield return null;
        }

        // Ensure max scale and highlight color reached
        textTransform.localScale = scoreNumberText2DOriginalScale * scoreScaleMultiplier;
        scoreNumberText2D.color = scoreHighlightColor;

        elapsedTime = 0f;

        // Phase 2: Scale down and restore original color
        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / halfDuration;

            textTransform.localScale = Vector3.Lerp(scoreNumberText2DOriginalScale * scoreScaleMultiplier, scoreNumberText2DOriginalScale, t);
            scoreNumberText2D.color = Color.Lerp(scoreHighlightColor, scoreNumberText2DOriginalColor, t);

            yield return null;
        }

        // Ensure original scale and color restored
        textTransform.localScale = scoreNumberText2DOriginalScale;
        scoreNumberText2D.color = scoreNumberText2DOriginalColor;
    }

    /// <summary>
    /// Coroutine that flashes the score text red during game over.
    /// Fades to red, holds, then returns to original color.
    /// </summary>
    private IEnumerator ScoreTextRedFlashCoroutine()
    {
        float elapsedTime = 0f;

        // Phase 1: Fade to red
        while (elapsedTime < scoreGameOverTransitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / scoreGameOverTransitionDuration;

            if (scoreText2D != null)
            {
                scoreText2D.color = Color.Lerp(scoreText2DOriginalColor, scoreGameOverColor, t);
            }

            if (scoreNumberText2D != null)
            {
                scoreNumberText2D.color = Color.Lerp(scoreNumberText2DOriginalColor, scoreGameOverColor, t);
            }

            yield return null;
        }

        // Ensure full red color reached
        if (scoreText2D != null)
        {
            scoreText2D.color = scoreGameOverColor;
        }

        if (scoreNumberText2D != null)
        {
            scoreNumberText2D.color = scoreGameOverColor;
        }

        // Phase 2: Hold red color
        yield return new WaitForSeconds(scoreGameOverHoldDuration);

        // Phase 3: Fade back to original colors
        elapsedTime = 0f;
        while (elapsedTime < scoreGameOverTransitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / scoreGameOverTransitionDuration;

            if (scoreText2D != null)
            {
                scoreText2D.color = Color.Lerp(scoreGameOverColor, scoreText2DOriginalColor, t);
            }

            if (scoreNumberText2D != null)
            {
                scoreNumberText2D.color = Color.Lerp(scoreGameOverColor, scoreNumberText2DOriginalColor, t);
            }

            yield return null;
        }

        // Ensure original colors are fully restored
        if (scoreText2D != null)
        {
            scoreText2D.color = scoreText2DOriginalColor;
        }

        if (scoreNumberText2D != null)
        {
            scoreNumberText2D.color = scoreNumberText2DOriginalColor;
        }
    }

    #endregion

    #region High Score Management

    /// <summary>
    /// Saves the current player score to the high score data.
    /// Called when the game ends.
    /// </summary>
    private void SaveHighScore()
    {
        DifficultyScores difficultyScores = GetDifficultyScores();

        if (difficultyScores != null)
        {
            // Add score with today's date
            difficultyScores.AddScore(playerScore);

            // Save to disk
            SaveHighScoresToDisk();

            if (enableDebugLogs)
            {
                Debug.Log($"Score saved for {difficulty}: {playerScore}");
            }
        }
    }

    /// <summary>
    /// Saves the high score data to disk as JSON.
    /// </summary>
    private void SaveHighScoresToDisk()
    {
        try
        {
            // Convert high score data to JSON
            string json = JsonUtility.ToJson(highScoreData, true);
            File.WriteAllText(saveFilePath, json);

            if (enableDebugLogs)
            {
                Debug.Log($"High scores saved to: {saveFilePath}");
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
            {
                Debug.LogError($"Failed to save high scores: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Loads high score data from disk.
    /// Creates a new HighScoreData if no save file exists.
    /// </summary>
    private void LoadHighScores()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                // Load and parse JSON from file
                string json = File.ReadAllText(saveFilePath);
                highScoreData = JsonUtility.FromJson<HighScoreData>(json);

                if (enableDebugLogs)
                {
                    Debug.Log($"High scores loaded from: {saveFilePath}");
                }
            }
            else
            {
                // No save file exists, create new data
                if (enableDebugLogs)
                {
                    Debug.Log("No save file found, starting with default high scores.");
                }
                highScoreData = new HighScoreData();
            }
        }
        catch (System.Exception e)
        {
            // Error loading, start with new data
            if (enableDebugLogs)
            {
                Debug.LogError($"Failed to load high scores: {e.Message}");
            }
            highScoreData = new HighScoreData();
        }
    }

    /// <summary>
    /// Gets the DifficultyScores object for the current difficulty level.
    /// </summary>
    /// <returns>The DifficultyScores for the current difficulty, or null if invalid</returns>
    private DifficultyScores GetDifficultyScores()
    {
        switch (difficulty)
        {
            case Difficulty.Easy:
                return highScoreData.easyScores;
            case Difficulty.Medium:
                return highScoreData.mediumScores;
            case Difficulty.Hard:
                return highScoreData.hardScores;
            default:
                return null;
        }
    }

    /// <summary>
    /// Gets the best score ever achieved for the current difficulty.
    /// </summary>
    /// <returns>The highest score of all time, or 0 if none</returns>
    public int GetBestScoreAllTime()
    {
        DifficultyScores difficultyScores = GetDifficultyScores();
        return difficultyScores != null ? difficultyScores.GetBestScoreAllTime() : 0;
    }

    /// <summary>
    /// Gets the best score achieved today for the current difficulty.
    /// </summary>
    /// <returns>The highest score today, or 0 if none</returns>
    public int GetBestScoreToday()
    {
        DifficultyScores difficultyScores = GetDifficultyScores();
        return difficultyScores != null ? difficultyScores.GetBestScoreToday() : 0;
    }

    /// <summary>
    /// Gets the best score achieved this week for the current difficulty.
    /// </summary>
    /// <returns>The highest score this week, or 0 if none</returns>
    public int GetBestScoreThisWeek()
    {
        DifficultyScores difficultyScores = GetDifficultyScores();
        return difficultyScores != null ? difficultyScores.GetBestScoreThisWeek() : 0;
    }

    /// <summary>
    /// Gets all high score data across all difficulty levels.
    /// </summary>
    /// <returns>The complete HighScoreData object</returns>
    public HighScoreData GetAllHighScores()
    {
        return highScoreData;
    }

    #endregion

    #region Scene Navigation

    /// <summary>
    /// Called when the Home button is pressed.
    /// Returns to the main menu scene with fade transition.
    /// </summary>
    public void OnHomeButtonPressed()
    {
        // Fade out and load the menu scene
        if (SceneFadeManager.Instance != null)
        {
            SceneFadeManager.Instance.FadeOutAndLoadScene(menuSceneName);
        }
        else
        {
            // Fallback if no fade manager exists
            SceneManager.LoadScene(menuSceneName);
        }
    }

    /// <summary>
    /// Called when the Play Again button is pressed.
    /// Restarts the game with the current difficulty with fade transition.
    /// </summary>
    public void OnPlayAgainButtonPressed()
    {
        // Fade out and reload the current scene
        if (SceneFadeManager.Instance != null)
        {
            SceneFadeManager.Instance.FadeOutAndLoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            // Fallback if no fade manager exists - restart without scene reload
            StartNewGame();
        }
    }

    #endregion
}
