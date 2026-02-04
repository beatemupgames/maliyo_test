using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

/// <summary>
/// Helper component that allows triggering scene fades from Unity events (like Button.onClick).
/// Attach this to GameObjects that need to trigger fade transitions without modifying existing code.
/// Can be used with UnityEvents to trigger fades before scene transitions.
/// </summary>
public class SceneFadeController : MonoBehaviour
{
    #region Serialized Fields

    [Header("Scene Transition")]
    [Tooltip("Name of the scene to load after fade out (leave empty to not load a scene)")]
    [SerializeField] private string targetSceneName = "";

    [Header("Events")]
    [Tooltip("Event triggered when fade out completes")]
    [SerializeField] private UnityEvent onFadeOutComplete;

    [Tooltip("Event triggered when fade in completes")]
    [SerializeField] private UnityEvent onFadeInComplete;

    #endregion

    #region Public Methods - Called from UnityEvents

    /// <summary>
    /// Triggers a fade in effect.
    /// Can be called from Button.onClick or other UnityEvents.
    /// </summary>
    public void TriggerFadeIn()
    {
        if (SceneFadeManager.Instance != null)
        {
            SceneFadeManager.Instance.FadeIn();

            // Optionally invoke callback when fade completes
            if (onFadeInComplete != null && onFadeInComplete.GetPersistentEventCount() > 0)
            {
                StartCoroutine(InvokeFadeInCompleteCoroutine());
            }
        }
    }

    /// <summary>
    /// Triggers a fade out effect.
    /// Can be called from Button.onClick or other UnityEvents.
    /// If targetSceneName is set, will load that scene after fade completes.
    /// </summary>
    public void TriggerFadeOut()
    {
        if (SceneFadeManager.Instance != null)
        {
            SceneFadeManager.Instance.FadeOut();

            // Start coroutine to handle scene loading and callback
            StartCoroutine(HandleFadeOutCoroutine());
        }
    }

    /// <summary>
    /// Triggers a fade out and loads the target scene.
    /// Can be called from Button.onClick or other UnityEvents.
    /// Convenience method that combines fade out with scene loading.
    /// </summary>
    public void TriggerFadeOutAndLoadScene()
    {
        if (SceneFadeManager.Instance != null && !string.IsNullOrEmpty(targetSceneName))
        {
            SceneFadeManager.Instance.FadeOutAndLoadScene(targetSceneName);
        }
        else if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("SceneFadeController: No target scene name specified!");
        }
    }

    /// <summary>
    /// Triggers a fade out and loads a specific scene.
    /// Can be called from Button.onClick or other UnityEvents.
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    public void TriggerFadeOutAndLoadScene(string sceneName)
    {
        if (SceneFadeManager.Instance != null && !string.IsNullOrEmpty(sceneName))
        {
            SceneFadeManager.Instance.FadeOutAndLoadScene(sceneName);
        }
        else if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("SceneFadeController: No scene name provided!");
        }
    }

    #endregion

    #region Private Methods - Coroutines

    /// <summary>
    /// Waits for fade out to complete, then loads scene and/or invokes callback.
    /// </summary>
    private System.Collections.IEnumerator HandleFadeOutCoroutine()
    {
        // Wait for fade to complete
        while (SceneFadeManager.Instance != null && SceneFadeManager.Instance.IsFading())
        {
            yield return null;
        }

        // Load scene if specified
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }

        // Invoke callback if set
        if (onFadeOutComplete != null)
        {
            onFadeOutComplete.Invoke();
        }
    }

    /// <summary>
    /// Waits for fade in to complete, then invokes callback.
    /// </summary>
    private System.Collections.IEnumerator InvokeFadeInCompleteCoroutine()
    {
        // Wait for fade to complete
        while (SceneFadeManager.Instance != null && SceneFadeManager.Instance.IsFading())
        {
            yield return null;
        }

        // Invoke callback
        if (onFadeInComplete != null)
        {
            onFadeInComplete.Invoke();
        }
    }

    #endregion
}
