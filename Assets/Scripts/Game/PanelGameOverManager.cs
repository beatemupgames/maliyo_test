using System.Collections;
using UnityEngine;

public class PanelGameOverManager : MonoBehaviour
{
    #region Serialized Fields

    [Header("Panel Game Over Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.15f;

    #endregion

    #region Private Fields

    private GameManager gameManager;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Called when the script instance is loaded.
    /// Finds and caches the GameManager reference.
    /// </summary>
    private void Awake()
    {
        // Find GameManager in the scene
        gameManager = FindFirstObjectByType<GameManager>();
    }

    #endregion

    #region Sound Effects

    /// <summary>
    /// Plays the game over sound effect.
    /// Called when the game over sequence starts.
    /// </summary>
    public void PlayGameOverSound()
    {
        // Play game over sound through SoundManager
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("GameOver");
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

    #region Fade Animations

    /// <summary>
    /// Coroutine that fades in the game over panel.
    /// Animates the CanvasGroup alpha from 0 to 1.
    /// </summary>
    /// <returns>IEnumerator for coroutine execution</returns>
    public IEnumerator FadeInCoroutine()
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
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            yield return null;
        }

        // Ensure fully opaque at end
        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Coroutine that fades out the game over panel.
    /// Animates the CanvasGroup alpha from 1 to 0.
    /// </summary>
    /// <returns>IEnumerator for coroutine execution</returns>
    public IEnumerator FadeOutCoroutine()
    {
        // Get or add CanvasGroup component for alpha control
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Start fully opaque
        float elapsedTime = 0f;
        canvasGroup.alpha = 1f;

        // Fade out over duration
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
            yield return null;
        }

        // Ensure fully transparent at end
        canvasGroup.alpha = 0f;
    }

    #endregion

    #region Panel Visibility

    /// <summary>
    /// Shows the game over panel by activating the GameObject.
    /// Called when the game is over to display the panel.
    /// </summary>
    public void ShowPanel()
    {
        // Activate the panel GameObject
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Hides the game over panel by deactivating the GameObject.
    /// Called when transitioning away from the game over state.
    /// </summary>
    public void HidePanel()
    {
        // Deactivate the panel GameObject
        gameObject.SetActive(false);
    }

    #endregion

    #region Button Callbacks

    /// <summary>
    /// Called when the Restart button is clicked.
    /// Plays click sound and triggers game restart through GameManager.
    /// </summary>
    public void OnRestartButton()
    {
        // Play button click sound
        PlayClickSound();

        // Request game restart from GameManager
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
    }

    /// <summary>
    /// Called when the No Thanks button is clicked.
    /// Plays click sound and triggers the no thanks action through GameManager.
    /// </summary>
    public void OnNoThanksButton()
    {
        // Play button click sound
        PlayClickSound();

        // Notify GameManager of no thanks action
        if (gameManager != null)
        {
            gameManager.OnNoThanksButtonPressed();
        }
    }

    #endregion
}
