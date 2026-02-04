using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Manages scene transition fades (fade in/out effects).
/// This singleton persists across scenes and automatically handles fade in when scenes load.
/// Use FadeOut() before loading a new scene for smooth transitions.
/// </summary>
public class SceneFadeManager : MonoBehaviour
{
    #region Singleton

    private static SceneFadeManager instance;

    /// <summary>
    /// Singleton instance of the SceneFadeManager.
    /// Accessible from anywhere in the game.
    /// </summary>
    public static SceneFadeManager Instance
    {
        get { return instance; }
    }

    #endregion

    #region Serialized Fields

    [Header("Fade Settings")]
    [Tooltip("Duration of the fade in effect (seconds)")]
    [SerializeField] private float fadeInDuration = 0.5f;

    [Tooltip("Duration of the fade out effect (seconds)")]
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Tooltip("Color of the fade overlay (usually black)")]
    [SerializeField] private Color fadeColor = Color.black;

    [Tooltip("Animation curve for fade transitions (controls easing)")]
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("Automatically fade in when a scene loads")]
    [SerializeField] private bool autoFadeInOnSceneLoad = true;

    [Header("UI References")]
    [Tooltip("Canvas that will display the fade overlay")]
    [SerializeField] private Canvas fadeCanvas;

    [Tooltip("Image component used as the fade overlay")]
    [SerializeField] private Image fadeImage;

    #endregion

    #region Private Fields

    private bool isFading = false;
    private Coroutine currentFadeCoroutine;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Called when the script instance is loaded.
    /// Sets up the singleton and registers scene load callback.
    /// </summary>
    private void Awake()
    {
        // Singleton pattern - ensure only one instance exists
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure canvas is set to overlay mode and on top of everything
        if (fadeCanvas != null)
        {
            fadeCanvas.sortingOrder = 999;
            fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        // Start with fade image invisible
        if (fadeImage != null)
        {
            Color startColor = fadeColor;
            startColor.a = 1f; // Start fully opaque (will fade in on first scene)
            fadeImage.color = startColor;
        }

        // Register callback for scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// Called when the MonoBehaviour is destroyed.
    /// Cleans up event subscriptions.
    /// </summary>
    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    #endregion

    #region Scene Loading Callback

    /// <summary>
    /// Called automatically when a new scene finishes loading.
    /// Triggers the fade in effect if auto fade is enabled.
    /// </summary>
    /// <param name="scene">The scene that was loaded</param>
    /// <param name="mode">The scene load mode used</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (autoFadeInOnSceneLoad)
        {
            FadeIn();
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Starts a fade in effect (from opaque to transparent).
    /// Use this when entering a scene.
    /// </summary>
    public void FadeIn()
    {
        if (fadeImage == null) return;

        // Stop any current fade
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        currentFadeCoroutine = StartCoroutine(FadeCoroutine(1f, 0f, fadeInDuration));
    }

    /// <summary>
    /// Starts a fade out effect (from transparent to opaque).
    /// Use this before leaving a scene.
    /// </summary>
    public void FadeOut()
    {
        if (fadeImage == null) return;

        // Stop any current fade
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        currentFadeCoroutine = StartCoroutine(FadeCoroutine(fadeImage.color.a, 1f, fadeOutDuration));
    }

    /// <summary>
    /// Starts a fade out effect and loads a new scene when complete.
    /// Convenience method that combines fade out with scene loading.
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    public void FadeOutAndLoadScene(string sceneName)
    {
        if (fadeImage == null)
        {
            // If no fade image, just load the scene directly
            SceneManager.LoadScene(sceneName);
            return;
        }

        // Stop any current fade
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        currentFadeCoroutine = StartCoroutine(FadeOutAndLoadSceneCoroutine(sceneName));
    }

    /// <summary>
    /// Checks if a fade transition is currently in progress.
    /// </summary>
    /// <returns>True if fading, false otherwise</returns>
    public bool IsFading()
    {
        return isFading;
    }

    #endregion

    #region Private Methods - Coroutines

    /// <summary>
    /// Coroutine that performs the fade transition over time.
    /// </summary>
    /// <param name="startAlpha">Starting alpha value (0-1)</param>
    /// <param name="endAlpha">Ending alpha value (0-1)</param>
    /// <param name="duration">Duration of the fade in seconds</param>
    private IEnumerator FadeCoroutine(float startAlpha, float endAlpha, float duration)
    {
        isFading = true;

        float elapsed = 0f;
        Color color = fadeColor;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Apply animation curve for smooth easing
            float curveValue = fadeCurve.Evaluate(t);

            // Interpolate alpha value
            float alpha = Mathf.Lerp(startAlpha, endAlpha, curveValue);
            color.a = alpha;
            fadeImage.color = color;

            yield return null;
        }

        // Ensure final alpha is set precisely
        color.a = endAlpha;
        fadeImage.color = color;

        isFading = false;
        currentFadeCoroutine = null;
    }

    /// <summary>
    /// Coroutine that fades out and then loads a new scene.
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    private IEnumerator FadeOutAndLoadSceneCoroutine(string sceneName)
    {
        isFading = true;

        float elapsed = 0f;
        Color color = fadeColor;
        float startAlpha = fadeImage.color.a;

        // Fade out
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);

            // Apply animation curve for smooth easing
            float curveValue = fadeCurve.Evaluate(t);

            // Interpolate alpha value
            float alpha = Mathf.Lerp(startAlpha, 1f, curveValue);
            color.a = alpha;
            fadeImage.color = color;

            yield return null;
        }

        // Ensure fully opaque before loading scene
        color.a = 1f;
        fadeImage.color = color;

        isFading = false;
        currentFadeCoroutine = null;

        // Load the new scene
        SceneManager.LoadScene(sceneName);
    }

    #endregion
}
