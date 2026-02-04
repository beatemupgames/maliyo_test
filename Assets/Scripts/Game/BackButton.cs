using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour
{
    #region Serialized Fields

    [Header("Scene Navigation")]
    [SerializeField] private string targetSceneName = "Menu";

    #endregion

    #region Public Methods

    /// <summary>
    /// Navigates back to the target scene (typically the main menu) with fade transition.
    /// Called when the back button is clicked.
    /// </summary>
    public void GoBack()
    {
        // Fade out and load the target scene
        if (SceneFadeManager.Instance != null)
        {
            SceneFadeManager.Instance.FadeOutAndLoadScene(targetSceneName);
        }
        else
        {
            // Fallback if no fade manager exists
            SceneManager.LoadScene(targetSceneName);
        }
    }

    #endregion
}
