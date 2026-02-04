using System.Collections;
using UnityEngine;
using TMPro;

public class PanelScoreManager : MonoBehaviour
{
    [Header("Panel Score References")]
    [SerializeField] private GameObject triangle1;
    [SerializeField] private GameObject triangle2;
    [SerializeField] private TextMeshProUGUI panelScoreCurrentScoreText;
    [SerializeField] private TextMeshProUGUI panelScoreBestTodayText;
    [SerializeField] private TextMeshProUGUI panelScoreBestWeekText;
    [SerializeField] private TextMeshProUGUI panelScoreBestAllTimeText;
    [SerializeField] private TextMeshProUGUI panelScoreDifficultyText;

    [Header("Panel Score Settings")]
    [SerializeField] private Color easyDifficultyColor = new Color(0.184f, 0.718f, 0.255f); // #2FB741
    [SerializeField] private Color mediumDifficultyColor = new Color(0.992f, 0.655f, 0.016f); // #FDA704
    [SerializeField] private Color hardDifficultyColor = new Color(0.996f, 0.373f, 0.337f); // #FE5F56
    [SerializeField] private float triangleRotationDuration = 0.5f;
    [SerializeField] private float triangleFadeDuration = 0.3f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip panelScoreSound;

    private Coroutine triangle1RotationCoroutine;
    private Coroutine triangle2RotationCoroutine;

    private void Awake()
    {
        InitializeTriangles();
    }

    private void InitializeTriangles()
    {
        InitializeTriangle(triangle1);
        InitializeTriangle(triangle2);
    }

    private void InitializeTriangle(GameObject triangle)
    {
        if (triangle != null)
        {
            CanvasGroup triangleCanvasGroup = triangle.GetComponent<CanvasGroup>();
            if (triangleCanvasGroup == null)
            {
                triangleCanvasGroup = triangle.AddComponent<CanvasGroup>();
            }
            triangleCanvasGroup.alpha = 0f;
            triangle.SetActive(false);
        }
    }

    public void ResetPanel()
    {
        StopTriangleAnimations();
        ResetTriangle(triangle1);
        ResetTriangle(triangle2);
    }

    private void ResetTriangle(GameObject triangle)
    {
        if (triangle != null)
        {
            CanvasGroup canvasGroup = triangle.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
            triangle.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            triangle.SetActive(false);
        }
    }

    public void UpdatePanelScoreUI(int playerScore, int bestToday, int bestWeek, int bestAllTime, GameManager.Difficulty difficulty)
    {
        if (panelScoreCurrentScoreText != null)
        {
            panelScoreCurrentScoreText.text = playerScore.ToString();
        }

        if (panelScoreBestTodayText != null)
        {
            panelScoreBestTodayText.text = bestToday.ToString();
        }

        if (panelScoreBestWeekText != null)
        {
            panelScoreBestWeekText.text = bestWeek.ToString();
        }

        if (panelScoreBestAllTimeText != null)
        {
            panelScoreBestAllTimeText.text = bestAllTime.ToString();
        }

        UpdateDifficultyText(difficulty);
    }

    private void UpdateDifficultyText(GameManager.Difficulty difficulty)
    {
        if (panelScoreDifficultyText != null)
        {
            panelScoreDifficultyText.text = difficulty.ToString().ToUpper();

            Color difficultyColor = mediumDifficultyColor;
            switch (difficulty)
            {
                case GameManager.Difficulty.Easy:
                    difficultyColor = easyDifficultyColor;
                    break;
                case GameManager.Difficulty.Medium:
                    difficultyColor = mediumDifficultyColor;
                    break;
                case GameManager.Difficulty.Hard:
                    difficultyColor = hardDifficultyColor;
                    break;
            }
            panelScoreDifficultyText.color = difficultyColor;
        }
    }

    public void ShowPanel()
    {
        gameObject.SetActive(true);
    }

    public void PlayPanelScoreSound()
    {
        if (audioSource != null && panelScoreSound != null)
        {
            audioSource.PlayOneShot(panelScoreSound);
        }
    }

    public void ActivateTriangles()
    {
        if (triangle1 != null)
        {
            triangle1.SetActive(true);
        }

        if (triangle2 != null)
        {
            triangle2.SetActive(true);
        }
    }

    public void StartTriangleAnimations()
    {
        triangle1RotationCoroutine = StartCoroutine(Triangle1AnimationCoroutine());
        triangle2RotationCoroutine = StartCoroutine(Triangle2AnimationCoroutine());
    }

    public void StopTriangleAnimations()
    {
        if (triangle1RotationCoroutine != null)
        {
            StopCoroutine(triangle1RotationCoroutine);
            triangle1RotationCoroutine = null;
        }

        if (triangle2RotationCoroutine != null)
        {
            StopCoroutine(triangle2RotationCoroutine);
            triangle2RotationCoroutine = null;
        }
    }

    private IEnumerator Triangle1AnimationCoroutine()
    {
        if (triangle1 == null) yield break;

        // Get or add CanvasGroup
        CanvasGroup canvasGroup = triangle1.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = triangle1.AddComponent<CanvasGroup>();
        }

        // Get RectTransform for rotation
        RectTransform rectTransform = triangle1.GetComponent<RectTransform>();
        if (rectTransform == null) yield break;

        // Set pivot to top center (0.5, 1)
        rectTransform.pivot = new Vector2(0.5f, 1f);

        // Start at center (0 degrees)
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        // Fade in
        float elapsedTime = 0f;
        canvasGroup.alpha = 0f;

        while (elapsedTime < triangleFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / triangleFadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;

        // Pendulum rotation loop: 0 to -45 (left) back to 0, repeat
        while (true)
        {
            // Move 45 degrees to the left
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

            rectTransform.localRotation = Quaternion.Euler(0f, 0f, -45f);

            // Move 45 degrees to the right (back to initial position)
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

            rectTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    private IEnumerator Triangle2AnimationCoroutine()
    {
        if (triangle2 == null) yield break;

        // Get or add CanvasGroup
        CanvasGroup canvasGroup = triangle2.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = triangle2.AddComponent<CanvasGroup>();
        }

        // Get RectTransform for rotation
        RectTransform rectTransform = triangle2.GetComponent<RectTransform>();
        if (rectTransform == null) yield break;

        // Set pivot to top center (0.5, 1)
        rectTransform.pivot = new Vector2(0.5f, 1f);

        // Start at center (0 degrees)
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        // Fade in
        float elapsedTime = 0f;
        canvasGroup.alpha = 0f;

        while (elapsedTime < triangleFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / triangleFadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;

        // Wait for 0.5 second delay
        yield return new WaitForSeconds(0.5f);

        // Pendulum rotation loop: 0 to +45 (right) back to 0, repeat (inverse of triangle1)
        while (true)
        {
            // Move 45 degrees to the right
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

            rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);

            // Move 45 degrees to the left (back to initial position)
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

            rectTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    public IEnumerator FadeInPanelCoroutine(float duration)
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        float elapsedTime = 0f;
        canvasGroup.alpha = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }
}
