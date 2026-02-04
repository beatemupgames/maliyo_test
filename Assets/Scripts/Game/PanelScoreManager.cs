using System.Collections;
using UnityEngine;
using TMPro;

public class PanelScoreManager : MonoBehaviour
{
    #region Serialized Fields

    [Header("Panel Score References")]
    [SerializeField] private GameObject triangle1;
    [SerializeField] private GameObject triangle2;
    [SerializeField] private TextMeshProUGUI panelScoreCurrentScoreText;
    [SerializeField] private TextMeshProUGUI panelScoreBestTodayText;
    [SerializeField] private TextMeshProUGUI panelScoreBestWeekText;
    [SerializeField] private TextMeshProUGUI panelScoreBestAllTimeText;
    [SerializeField] private TextMeshProUGUI panelScoreDifficultyText;
    [SerializeField] private UnityEngine.UI.Image panelScoreDifficultyIcon;
    [SerializeField] private TextMeshProUGUI textMode;
    [SerializeField] private UnityEngine.UI.Image difficultyIcon;

    [Header("Panel Score Settings")]
    [SerializeField] private DifficultyColorsConfig colorsConfig;
    [SerializeField] private Sprite easyDifficultySprite;
    [SerializeField] private Sprite mediumDifficultySprite;
    [SerializeField] private Sprite hardDifficultySprite;
    [SerializeField] private float triangleRotationDuration = 0.5f;
    [SerializeField] private float triangleFadeDuration = 0.3f;
    [SerializeField] private float scoreWaitDuration = 1.0f;
    [SerializeField] private float scoreFadeInDuration = 0.3f;

    #endregion

    #region Private Fields

    private Coroutine triangle1RotationCoroutine;
    private Coroutine triangle2RotationCoroutine;
    private GameManager gameManager;

    #endregion

    #region Properties

    public float ScoreWaitDuration => scoreWaitDuration;
    public float ScoreFadeInDuration => scoreFadeInDuration;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Called when the script instance is loaded.
    /// Initializes triangles and finds the GameManager reference.
    /// </summary>
    private void Awake()
    {
        // Initialize triangle visual elements
        InitializeTriangles();

        // Find GameManager in the scene
        gameManager = FindFirstObjectByType<GameManager>();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes both triangle GameObjects by adding CanvasGroups and setting their initial state.
    /// </summary>
    private void InitializeTriangles()
    {
        // Initialize both triangles
        InitializeTriangle(triangle1);
        InitializeTriangle(triangle2);
    }

    /// <summary>
    /// Initializes a single triangle GameObject by adding a CanvasGroup and hiding it.
    /// </summary>
    /// <param name="triangle">The triangle GameObject to initialize</param>
    private void InitializeTriangle(GameObject triangle)
    {
        if (triangle != null)
        {
            // Get or add CanvasGroup component for alpha control
            CanvasGroup triangleCanvasGroup = triangle.GetComponent<CanvasGroup>();
            if (triangleCanvasGroup == null)
            {
                triangleCanvasGroup = triangle.AddComponent<CanvasGroup>();
            }

            // Start fully transparent and inactive
            triangleCanvasGroup.alpha = 0f;
            triangle.SetActive(false);
        }
    }

    #endregion

    #region Panel Management

    /// <summary>
    /// Resets the panel to its initial state.
    /// Stops all animations and resets triangle positions and alpha.
    /// </summary>
    public void ResetPanel()
    {
        // Stop any running triangle animations
        StopTriangleAnimations();

        // Reset both triangles to initial state
        ResetTriangle(triangle1);
        ResetTriangle(triangle2);
    }

    /// <summary>
    /// Resets a single triangle to its initial state.
    /// Sets alpha to 0, rotation to 0, and deactivates the GameObject.
    /// </summary>
    /// <param name="triangle">The triangle GameObject to reset</param>
    private void ResetTriangle(GameObject triangle)
    {
        if (triangle != null)
        {
            // Reset alpha to fully transparent
            CanvasGroup canvasGroup = triangle.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            // Reset rotation to default position
            triangle.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

            // Deactivate the GameObject
            triangle.SetActive(false);
        }
    }

    /// <summary>
    /// Shows the score panel by activating the GameObject.
    /// Called when transitioning to the score display.
    /// </summary>
    public void ShowPanel()
    {
        // Activate the panel GameObject
        gameObject.SetActive(true);
    }

    #endregion

    #region Score UI Update

    /// <summary>
    /// Updates all score text elements and difficulty display on the panel.
    /// </summary>
    /// <param name="playerScore">The player's current score</param>
    /// <param name="bestToday">The best score achieved today</param>
    /// <param name="bestWeek">The best score achieved this week</param>
    /// <param name="bestAllTime">The best score of all time</param>
    /// <param name="difficulty">The current difficulty level</param>
    public void UpdatePanelScoreUI(int playerScore, int bestToday, int bestWeek, int bestAllTime, GameManager.Difficulty difficulty)
    {
        // Update current score text
        if (panelScoreCurrentScoreText != null)
        {
            panelScoreCurrentScoreText.text = playerScore.ToString();
        }

        // Update best score today text
        if (panelScoreBestTodayText != null)
        {
            panelScoreBestTodayText.text = bestToday.ToString();
        }

        // Update best score this week text
        if (panelScoreBestWeekText != null)
        {
            panelScoreBestWeekText.text = bestWeek.ToString();
        }

        // Update best score all time text
        if (panelScoreBestAllTimeText != null)
        {
            panelScoreBestAllTimeText.text = bestAllTime.ToString();
        }

        // Update difficulty display (color and text)
        UpdateDifficultyText(difficulty);
    }

    /// <summary>
    /// Updates the difficulty text and icon on the score panel based on the current difficulty.
    /// </summary>
    /// <param name="difficulty">The current difficulty level</param>
    private void UpdateDifficultyText(GameManager.Difficulty difficulty)
    {
        // Default values for medium difficulty
        Color difficultyColor = colorsConfig != null ? colorsConfig.MediumColor : Color.white;
        Sprite difficultySprite = mediumDifficultySprite;

        // Determine color and sprite based on difficulty
        switch (difficulty)
        {
            case GameManager.Difficulty.Easy:
                difficultyColor = colorsConfig != null ? colorsConfig.EasyColor : Color.green;
                difficultySprite = easyDifficultySprite;
                break;
            case GameManager.Difficulty.Medium:
                difficultyColor = colorsConfig != null ? colorsConfig.MediumColor : Color.yellow;
                difficultySprite = mediumDifficultySprite;
                break;
            case GameManager.Difficulty.Hard:
                difficultyColor = colorsConfig != null ? colorsConfig.HardColor : Color.red;
                difficultySprite = hardDifficultySprite;
                break;
        }

        // Update difficulty text with color
        if (panelScoreDifficultyText != null)
        {
            panelScoreDifficultyText.text = difficulty.ToString().ToUpper();
            panelScoreDifficultyText.color = difficultyColor;
        }

        // Update difficulty icon sprite
        if (panelScoreDifficultyIcon != null && difficultySprite != null)
        {
            panelScoreDifficultyIcon.sprite = difficultySprite;
        }
    }

    /// <summary>
    /// Updates the difficulty UI elements (text and icon) with color and sprite.
    /// Used for the HUD display during gameplay.
    /// </summary>
    /// <param name="difficulty">The current difficulty level</param>
    public void UpdateDifficultyUI(GameManager.Difficulty difficulty)
    {
        // Default values for medium difficulty
        Color difficultyColor = colorsConfig != null ? colorsConfig.MediumColor : Color.white;
        Sprite difficultySprite = mediumDifficultySprite;

        // Determine color and sprite based on difficulty
        switch (difficulty)
        {
            case GameManager.Difficulty.Easy:
                difficultyColor = colorsConfig != null ? colorsConfig.EasyColor : Color.green;
                difficultySprite = easyDifficultySprite;
                break;
            case GameManager.Difficulty.Medium:
                difficultyColor = colorsConfig != null ? colorsConfig.MediumColor : Color.yellow;
                difficultySprite = mediumDifficultySprite;
                break;
            case GameManager.Difficulty.Hard:
                difficultyColor = colorsConfig != null ? colorsConfig.HardColor : Color.red;
                difficultySprite = hardDifficultySprite;
                break;
        }

        // Update mode text color
        if (textMode != null)
        {
            textMode.color = difficultyColor;
        }

        // Update difficulty icon sprite
        if (difficultyIcon != null && difficultySprite != null)
        {
            difficultyIcon.sprite = difficultySprite;
        }
    }

    #endregion

    #region Sound Effects

    /// <summary>
    /// Plays the panel score sound effect.
    /// Called when the score panel is displayed.
    /// </summary>
    public void PlayPanelScoreSound()
    {
        // Play panel score sound through SoundManager
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("PanelScore");
        }
    }

    /// <summary>
    /// Plays the button click sound effect.
    /// Called when any button on the panel is clicked.
    /// </summary>
    public void PlayClickSound()
    {
        // Play click sound through SoundManager
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("Click");
        }
    }

    #endregion

    #region Triangle Animations

    /// <summary>
    /// Activates both triangle GameObjects to prepare them for animation.
    /// Called before starting the triangle animations.
    /// </summary>
    public void ActivateTriangles()
    {
        // Activate triangle 1
        if (triangle1 != null)
        {
            triangle1.SetActive(true);
        }

        // Activate triangle 2
        if (triangle2 != null)
        {
            triangle2.SetActive(true);
        }
    }

    /// <summary>
    /// Starts the pendulum rotation animations for both triangles.
    /// Triangle 2 starts with a 0.5 second delay and rotates in the opposite direction.
    /// </summary>
    public void StartTriangleAnimations()
    {
        // Start triangle 1 animation (swings left first)
        triangle1RotationCoroutine = StartCoroutine(Triangle1AnimationCoroutine());

        // Start triangle 2 animation (swings right first, with delay)
        triangle2RotationCoroutine = StartCoroutine(Triangle2AnimationCoroutine());
    }

    /// <summary>
    /// Stops all running triangle animations.
    /// Called when resetting or hiding the panel.
    /// </summary>
    public void StopTriangleAnimations()
    {
        // Stop triangle 1 animation
        if (triangle1RotationCoroutine != null)
        {
            StopCoroutine(triangle1RotationCoroutine);
            triangle1RotationCoroutine = null;
        }

        // Stop triangle 2 animation
        if (triangle2RotationCoroutine != null)
        {
            StopCoroutine(triangle2RotationCoroutine);
            triangle2RotationCoroutine = null;
        }
    }

    /// <summary>
    /// Coroutine that animates triangle 1 with a pendulum motion.
    /// Fades in, then swings left (-45°) and back to center (0°) continuously.
    /// </summary>
    private IEnumerator Triangle1AnimationCoroutine()
    {
        if (triangle1 == null) yield break;

        // Get or add CanvasGroup for alpha control
        CanvasGroup canvasGroup = triangle1.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = triangle1.AddComponent<CanvasGroup>();
        }

        // Get RectTransform for rotation
        RectTransform rectTransform = triangle1.GetComponent<RectTransform>();
        if (rectTransform == null) yield break;

        // Set pivot to top center for pendulum rotation
        rectTransform.pivot = new Vector2(0.5f, 1f);

        // Start at center position (0 degrees)
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        // Fade in animation
        float elapsedTime = 0f;
        canvasGroup.alpha = 0f;

        while (elapsedTime < triangleFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / triangleFadeDuration);
            yield return null;
        }

        // Ensure fully visible
        canvasGroup.alpha = 1f;

        // Pendulum rotation loop: 0° to -45° (left) back to 0°, repeat
        while (true)
        {
            // Swing 45 degrees to the left
            elapsedTime = 0f;
            float startAngle = 0f;
            float targetAngle = -45f;

            while (elapsedTime < triangleRotationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / triangleRotationDuration;

                // Use smoothstep for more natural pendulum motion
                float smoothT = t * t * (3f - 2f * t);
                float currentAngle = Mathf.Lerp(startAngle, targetAngle, smoothT);

                rectTransform.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
                yield return null;
            }

            // Ensure exact target angle
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, -45f);

            // Swing 45 degrees to the right (back to initial position)
            elapsedTime = 0f;
            startAngle = -45f;
            targetAngle = 0f;

            while (elapsedTime < triangleRotationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / triangleRotationDuration;

                // Use smoothstep for more natural pendulum motion
                float smoothT = t * t * (3f - 2f * t);
                float currentAngle = Mathf.Lerp(startAngle, targetAngle, smoothT);

                rectTransform.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
                yield return null;
            }

            // Ensure exact target angle
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    /// <summary>
    /// Coroutine that animates triangle 2 with a pendulum motion (opposite of triangle 1).
    /// Fades in, waits 0.5 seconds, then swings right (+45°) and back to center (0°) continuously.
    /// </summary>
    private IEnumerator Triangle2AnimationCoroutine()
    {
        if (triangle2 == null) yield break;

        // Get or add CanvasGroup for alpha control
        CanvasGroup canvasGroup = triangle2.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = triangle2.AddComponent<CanvasGroup>();
        }

        // Get RectTransform for rotation
        RectTransform rectTransform = triangle2.GetComponent<RectTransform>();
        if (rectTransform == null) yield break;

        // Set pivot to top center for pendulum rotation
        rectTransform.pivot = new Vector2(0.5f, 1f);

        // Start at center position (0 degrees)
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        // Fade in animation
        float elapsedTime = 0f;
        canvasGroup.alpha = 0f;

        while (elapsedTime < triangleFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / triangleFadeDuration);
            yield return null;
        }

        // Ensure fully visible
        canvasGroup.alpha = 1f;

        // Wait for 0.5 second delay (offset from triangle 1)
        yield return new WaitForSeconds(0.5f);

        // Pendulum rotation loop: 0° to +45° (right) back to 0°, repeat (inverse of triangle1)
        while (true)
        {
            // Swing 45 degrees to the right
            elapsedTime = 0f;
            float startAngle = 0f;
            float targetAngle = 45f;

            while (elapsedTime < triangleRotationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / triangleRotationDuration;

                // Use smoothstep for more natural pendulum motion
                float smoothT = t * t * (3f - 2f * t);
                float currentAngle = Mathf.Lerp(startAngle, targetAngle, smoothT);

                rectTransform.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
                yield return null;
            }

            // Ensure exact target angle
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);

            // Swing 45 degrees to the left (back to initial position)
            elapsedTime = 0f;
            startAngle = 45f;
            targetAngle = 0f;

            while (elapsedTime < triangleRotationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / triangleRotationDuration;

                // Use smoothstep for more natural pendulum motion
                float smoothT = t * t * (3f - 2f * t);
                float currentAngle = Mathf.Lerp(startAngle, targetAngle, smoothT);

                rectTransform.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
                yield return null;
            }

            // Ensure exact target angle
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    #endregion

    #region Fade Animations

    /// <summary>
    /// Coroutine that fades in the score panel.
    /// Animates the CanvasGroup alpha from 0 to 1 over the specified duration.
    /// </summary>
    /// <param name="duration">Duration of the fade in effect in seconds</param>
    /// <returns>IEnumerator for coroutine execution</returns>
    public IEnumerator FadeInPanelCoroutine(float duration)
    {
        // Get or add CanvasGroup component for alpha control
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Start fully transparent
        float elapsedTime = 0f;
        canvasGroup.alpha = 0f;

        // Fade in over duration
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            yield return null;
        }

        // Ensure fully opaque at end
        canvasGroup.alpha = 1f;
    }

    #endregion

    #region Button Callbacks

    /// <summary>
    /// Called when the Home button is clicked.
    /// Plays click sound and navigates back to the main menu.
    /// </summary>
    public void OnHomeButton()
    {
        // Play button click sound
        PlayClickSound();

        // Navigate to menu through GameManager
        if (gameManager != null)
        {
            gameManager.OnHomeButtonPressed();
        }
    }

    /// <summary>
    /// Called when the Play Again button is clicked.
    /// Plays click sound and restarts the game.
    /// </summary>
    public void OnPlayAgainButton()
    {
        // Play button click sound
        PlayClickSound();

        // Restart game through GameManager
        if (gameManager != null)
        {
            gameManager.OnPlayAgainButtonPressed();
        }
    }

    #endregion
}
