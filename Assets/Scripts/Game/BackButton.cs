using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour
{
    [SerializeField]
    private string targetSceneName = "Menu";

    public void GoBack()
    {
        SceneManager.LoadScene(targetSceneName);
    }
}
