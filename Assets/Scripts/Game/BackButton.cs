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
    /// Navigates back to the target scene (typically the main menu).
    /// Called when the back button is clicked.
    /// </summary>
    public void GoBack()
    {
        // Load the target scene
        SceneManager.LoadScene(targetSceneName);
    }

    #endregion
}
