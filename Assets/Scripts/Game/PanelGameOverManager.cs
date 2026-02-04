using System.Collections;
using UnityEngine;

public class PanelGameOverManager : MonoBehaviour
{
    [Header("Panel Game Over Settings")]
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.15f;

    private GameManager gameManager;

    private void Awake()
    {
        // Panel starts inactive, no initialization needed
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public void PlayGameOverSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("GameOver");
        }
    }

    public void PlayClickSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("Click");
        }
    }

    public IEnumerator FadeInCoroutine()
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        float elapsedTime = 0f;
        canvasGroup.alpha = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    public IEnumerator FadeOutCoroutine()
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        float elapsedTime = 0f;
        canvasGroup.alpha = 1f;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    public void ShowPanel()
    {
        gameObject.SetActive(true);
    }

    public void HidePanel()
    {
        gameObject.SetActive(false);
    }

    // Button callbacks
    public void OnRestartButton()
    {
        PlayClickSound();
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
    }

    public void OnNoThanksButton()
    {
        PlayClickSound();
        if (gameManager != null)
        {
            gameManager.OnNoThanksButtonPressed();
        }
    }
}
