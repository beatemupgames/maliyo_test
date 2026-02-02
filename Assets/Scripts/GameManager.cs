using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;

public class GameManager : MonoBehaviour
{
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

    [System.Serializable]
    public class DifficultyScores
    {
        public List<ScoreEntry> scores = new List<ScoreEntry>();

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

        public void AddScore(int score)
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            scores.Add(new ScoreEntry(score, today));
        }
    }

    [System.Serializable]
    public class HighScoreData
    {
        public DifficultyScores easyScores = new DifficultyScores();
        public DifficultyScores mediumScores = new DifficultyScores();
        public DifficultyScores hardScores = new DifficultyScores();
    }

    [Header("Button References")]
    [SerializeField] private SimonButton[] buttons = new SimonButton[4];

    [Header("UI References")]
    [SerializeField] private TextMeshPro scoreText2D;
    [SerializeField] private TextMeshProUGUI scoreTextUI;
    [SerializeField] private TextMeshProUGUI highScoreTextUI;
    [SerializeField] private TextMeshProUGUI textMode;
    [SerializeField] private TextMeshProUGUI modeText;
    [SerializeField] private UnityEngine.UI.Image difficultyIcon;
    [SerializeField] private GameObject panelGameOver;
    [SerializeField] private GameObject panelScore;
    [SerializeField] private TextMeshProUGUI panelScoreCurrentScoreText;
    [SerializeField] private TextMeshProUGUI panelScoreBestTodayText;
    [SerializeField] private TextMeshProUGUI panelScoreBestWeekText;
    [SerializeField] private TextMeshProUGUI panelScoreBestAllTimeText;
    [SerializeField] private TextMeshProUGUI panelScoreDifficultyText;

    [Header("Game Elements to Fade")]
    [SerializeField] private GameObject simonGameObject;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip panelScoreSound;
    [SerializeField] private AudioClip clickSound;

    [Header("Game Settings")]
    [SerializeField] private Difficulty difficulty = Difficulty.Medium;
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField] private Color easyDifficultyColor = new Color(0.184f, 0.718f, 0.255f); // #2FB741
    [SerializeField] private Color mediumDifficultyColor = new Color(0.992f, 0.655f, 0.016f); // #FDA704
    [SerializeField] private Color hardDifficultyColor = new Color(0.996f, 0.373f, 0.337f); // #FE5F56
    [SerializeField] private Sprite easyDifficultySprite;
    [SerializeField] private Sprite mediumDifficultySprite;
    [SerializeField] private Sprite hardDifficultySprite;
    [SerializeField] private float timeBetweenButtons = 0.6f;
    [SerializeField] private float buttonPressDuration = 0.4f;
    [SerializeField] private float gameOverFadeDuration = 0.3f;
    [SerializeField] private float gameOverFadeOutDuration = 0.15f;
    [SerializeField] private float scoreWaitDuration = 1.0f;
    [SerializeField] private float scoreFadeInDuration = 0.3f;
    [SerializeField] private float simultaneousButtonDelay = 0.05f;

    [Header("Game State")]
    [SerializeField] private GameState currentState = GameState.Idle;
    [SerializeField] private List<SequenceStep> sequence = new List<SequenceStep>();
    [SerializeField] private int currentPlayerStep = 0;
    [SerializeField] private int currentRound = 0;
    [SerializeField] private int playerScore = 0;

    private HashSet<ButtonColor> currentStepPressedButtons = new HashSet<ButtonColor>();
    private HighScoreData highScoreData = new HighScoreData();
    private string saveFilePath;

    public GameState CurrentState => currentState;
    public int CurrentRound => currentRound;
    public int PlayerScore => playerScore;

    private void Start()
    {
        // Load difficulty from PlayerPrefs if set
        if (PlayerPrefs.HasKey("GameDifficulty"))
        {
            string difficultyString = PlayerPrefs.GetString("GameDifficulty");
            if (System.Enum.TryParse(difficultyString, true, out Difficulty loadedDifficulty))
            {
                difficulty = loadedDifficulty;
            }
        }

        saveFilePath = Path.Combine(Application.persistentDataPath, "highscores.json");
        LoadHighScores();
        UpdateHighScoreUI();
        UpdateTextModeUI();

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(false);
        }

        if (panelScore != null)
        {
            panelScore.SetActive(false);
        }

        StartNewGame();
    }

    public void StartNewGame()
    {
        sequence.Clear();
        currentPlayerStep = 0;
        currentRound = 0;
        playerScore = 0;
        currentState = GameState.Idle;

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(false);
        }

        if (panelScore != null)
        {
            panelScore.SetActive(false);
        }

        RestoreGameElementsAlpha();
        UpdateScoreUI();
        StartNextRound();
    }

    private void StartNextRound()
    {
        currentRound++;
        currentPlayerStep = 0;
        AddNewButtonToSequence();
        StartCoroutine(ShowSequenceCoroutine());
    }

    private void AddNewButtonToSequence()
    {
        SequenceStep newStep;

        switch (difficulty)
        {
            case Difficulty.Easy:
                ButtonColor easyButton = GetRandomButtonForEasy();
                newStep = new SequenceStep(easyButton);
                break;

            case Difficulty.Medium:
                ButtonColor mediumButton = (ButtonColor)UnityEngine.Random.Range(0, 4);
                newStep = new SequenceStep(mediumButton);
                break;

            case Difficulty.Hard:
                if (UnityEngine.Random.value < 0.3f)
                {
                    List<ButtonColor> doubleButtons = new List<ButtonColor>();
                    ButtonColor first = (ButtonColor)UnityEngine.Random.Range(0, 4);
                    ButtonColor second;
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
                    ButtonColor hardButton = (ButtonColor)UnityEngine.Random.Range(0, 4);
                    newStep = new SequenceStep(hardButton);
                }
                break;

            default:
                ButtonColor defaultButton = (ButtonColor)UnityEngine.Random.Range(0, 4);
                newStep = new SequenceStep(defaultButton);
                break;
        }

        sequence.Add(newStep);
    }

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

    private IEnumerator ShowSequenceCoroutine()
    {
        currentState = GameState.ShowingSequence;
        SetButtonsInteractable(false);

        yield return new WaitForSeconds(0.5f);

        foreach (SequenceStep step in sequence)
        {
            if (step.buttons.Count == 1)
            {
                ActivateButton(step.buttons[0]);
                yield return new WaitForSeconds(buttonPressDuration);
                DeactivateButton(step.buttons[0]);
            }
            else if (step.buttons.Count > 1)
            {
                foreach (ButtonColor button in step.buttons)
                {
                    ActivateButton(button);
                    yield return new WaitForSeconds(simultaneousButtonDelay);
                }

                yield return new WaitForSeconds(buttonPressDuration - (simultaneousButtonDelay * step.buttons.Count));

                foreach (ButtonColor button in step.buttons)
                {
                    DeactivateButton(button);
                }
            }

            yield return new WaitForSeconds(timeBetweenButtons - buttonPressDuration);
        }

        currentState = GameState.WaitingForPlayerInput;
        currentStepPressedButtons.Clear();
        SetButtonsInteractable(true);
    }

    public void OnPlayerPressButton(ButtonColor buttonPressed)
    {
        if (currentState != GameState.WaitingForPlayerInput)
        {
            return;
        }

        StartCoroutine(HandlePlayerInputCoroutine(buttonPressed));
    }

    private IEnumerator HandlePlayerInputCoroutine(ButtonColor buttonPressed)
    {
        ActivateButton(buttonPressed);
        yield return new WaitForSeconds(buttonPressDuration);
        DeactivateButton(buttonPressed);

        if (CheckPlayerInput(buttonPressed))
        {
            OnPlayerCorrect();
        }
        else
        {
            OnPlayerFail();
        }
    }

    private bool CheckPlayerInput(ButtonColor buttonPressed)
    {
        SequenceStep currentStep = sequence[currentPlayerStep];

        if (currentStep.buttons.Count == 1)
        {
            return buttonPressed == currentStep.buttons[0];
        }
        else
        {
            if (currentStepPressedButtons.Contains(buttonPressed))
            {
                return false;
            }

            if (!currentStep.buttons.Contains(buttonPressed))
            {
                return false;
            }

            currentStepPressedButtons.Add(buttonPressed);

            return true;
        }
    }

    private void OnPlayerCorrect()
    {
        SequenceStep currentStep = sequence[currentPlayerStep];

        if (currentStep.buttons.Count > 1 && currentStepPressedButtons.Count < currentStep.buttons.Count)
        {
            return;
        }

        currentPlayerStep++;
        currentStepPressedButtons.Clear();

        if (currentPlayerStep >= sequence.Count)
        {
            playerScore++;
            UpdateScoreUI();
            StartNextRound();
        }
    }

    private void OnPlayerFail()
    {
        currentState = GameState.GameOver;
        SetButtonsInteractable(false);
        SaveHighScore();
        StartCoroutine(GameOverSequenceCoroutine());
        Debug.Log($"Game Over! Final Score: {playerScore}, Rounds Completed: {currentRound - 1}");
    }

    private IEnumerator GameOverSequenceCoroutine()
    {
        if (audioSource != null && gameOverSound != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }

        for (int i = 0; i < 2; i++)
        {
            foreach (SimonButton button in buttons)
            {
                if (button != null)
                {
                    button.ActivateGameOver();
                }
            }

            yield return new WaitForSeconds(0.3f);

            foreach (SimonButton button in buttons)
            {
                if (button != null)
                {
                    button.Deactivate();
                }
            }

            yield return new WaitForSeconds(0.3f);
        }

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
            yield return StartCoroutine(FadeInPanelCoroutine());
        }
    }

    private IEnumerator FadeInPanelCoroutine()
    {
        yield return StartCoroutine(FadeInPanelCoroutine(panelGameOver, gameOverFadeDuration));
    }

    private void ActivateButton(ButtonColor buttonColor)
    {
        SimonButton button = GetButtonByColor(buttonColor);
        if (button != null)
        {
            button.Activate();
        }
    }

    private void DeactivateButton(ButtonColor buttonColor)
    {
        SimonButton button = GetButtonByColor(buttonColor);
        if (button != null)
        {
            button.Deactivate();
        }
    }

    private SimonButton GetButtonByColor(ButtonColor color)
    {
        foreach (SimonButton button in buttons)
        {
            if (button != null && button.ButtonColor == color)
            {
                return button;
            }
        }
        return null;
    }

    private void SetButtonsInteractable(bool interactable)
    {
        foreach (SimonButton button in buttons)
        {
            if (button != null)
            {
                button.SetInteractable(interactable);
            }
        }
    }

    public void RestartGame()
    {
        if (currentState == GameState.GameOver)
        {
            StartNewGame();
        }
    }

    public void OnNoThanksButton()
    {
        if (currentState == GameState.GameOver)
        {
            PlayClickSound();
            StartCoroutine(NoThanksSequenceCoroutine());
        }
    }

    private IEnumerator NoThanksSequenceCoroutine()
    {
        if (panelGameOver != null)
        {
            yield return StartCoroutine(FadeOutPanelCoroutine(panelGameOver, gameOverFadeOutDuration));
            panelGameOver.SetActive(false);
        }

        yield return new WaitForSeconds(scoreWaitDuration);

        if (panelScore != null)
        {
            UpdatePanelScoreUI();
            panelScore.SetActive(true);

            if (audioSource != null && panelScoreSound != null)
            {
                audioSource.PlayOneShot(panelScoreSound);
            }

            StartCoroutine(FadeOutGameElementsCoroutine(scoreFadeInDuration));
            yield return StartCoroutine(FadeInPanelCoroutine(panelScore, scoreFadeInDuration));
        }
    }

    private IEnumerator FadeOutPanelCoroutine(GameObject panel, float duration)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
        }

        float elapsedTime = 0f;
        canvasGroup.alpha = 1f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    private IEnumerator FadeInPanelCoroutine(GameObject panel, float duration)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = panel.AddComponent<CanvasGroup>();
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

    private IEnumerator FadeOutGameElementsCoroutine(float duration)
    {
        SpriteRenderer[] simonSpriteRenderers = null;
        Color[] spriteOriginalColors = null;
        TextMeshPro[] simonTextMeshPros = null;
        Color[] textOriginalColors = null;

        if (simonGameObject != null)
        {
            simonSpriteRenderers = simonGameObject.GetComponentsInChildren<SpriteRenderer>();
            if (simonSpriteRenderers != null && simonSpriteRenderers.Length > 0)
            {
                spriteOriginalColors = new Color[simonSpriteRenderers.Length];
                for (int i = 0; i < simonSpriteRenderers.Length; i++)
                {
                    spriteOriginalColors[i] = simonSpriteRenderers[i].color;
                }
            }

            simonTextMeshPros = simonGameObject.GetComponentsInChildren<TextMeshPro>();
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

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);

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

            yield return null;
        }

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
    }

    private void RestoreGameElementsAlpha()
    {
        if (simonGameObject != null)
        {
            SpriteRenderer[] spriteRenderers = simonGameObject.GetComponentsInChildren<SpriteRenderer>();
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

            TextMeshPro[] textMeshPros = simonGameObject.GetComponentsInChildren<TextMeshPro>();
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
    }

    private void UpdateScoreUI()
    {
        string scoreString = playerScore.ToString();

        if (scoreText2D != null)
        {
            scoreText2D.text = scoreString;
        }

        if (scoreTextUI != null)
        {
            scoreTextUI.text = scoreString;
        }

        UpdateHighScoreUI();
    }

    private void UpdateHighScoreUI()
    {
        if (highScoreTextUI != null)
        {
            int savedHighScore = GetBestScoreAllTime();
            int displayScore = Mathf.Max(savedHighScore, playerScore);
            highScoreTextUI.text = displayScore.ToString();
        }
    }

    private void UpdateTextModeUI()
    {
        Color difficultyColor = mediumDifficultyColor;
        Sprite difficultySprite = mediumDifficultySprite;
        string modeTextString = "MEDIUM MODE";

        switch (difficulty)
        {
            case Difficulty.Easy:
                difficultyColor = easyDifficultyColor;
                difficultySprite = easyDifficultySprite;
                modeTextString = "EASY MODE";
                break;
            case Difficulty.Medium:
                difficultyColor = mediumDifficultyColor;
                difficultySprite = mediumDifficultySprite;
                modeTextString = "MEDIUM MODE";
                break;
            case Difficulty.Hard:
                difficultyColor = hardDifficultyColor;
                difficultySprite = hardDifficultySprite;
                modeTextString = "HARD MODE";
                break;
        }

        if (textMode != null)
        {
            textMode.color = difficultyColor;
        }

        if (modeText != null)
        {
            modeText.text = modeTextString;
        }

        if (difficultyIcon != null && difficultySprite != null)
        {
            difficultyIcon.sprite = difficultySprite;
        }
    }

    private void SaveHighScore()
    {
        DifficultyScores difficultyScores = GetDifficultyScores();

        if (difficultyScores != null)
        {
            difficultyScores.AddScore(playerScore);
            SaveHighScoresToDisk();
            Debug.Log($"Score saved for {difficulty}: {playerScore}");
        }
    }

    private void SaveHighScoresToDisk()
    {
        try
        {
            string json = JsonUtility.ToJson(highScoreData, true);
            File.WriteAllText(saveFilePath, json);
            Debug.Log($"High scores saved to: {saveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save high scores: {e.Message}");
        }
    }

    private void LoadHighScores()
    {
        try
        {
            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                highScoreData = JsonUtility.FromJson<HighScoreData>(json);
                Debug.Log($"High scores loaded from: {saveFilePath}");
            }
            else
            {
                Debug.Log("No save file found, starting with default high scores.");
                highScoreData = new HighScoreData();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load high scores: {e.Message}");
            highScoreData = new HighScoreData();
        }
    }

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

    public int GetBestScoreAllTime()
    {
        DifficultyScores difficultyScores = GetDifficultyScores();
        return difficultyScores != null ? difficultyScores.GetBestScoreAllTime() : 0;
    }

    public int GetBestScoreToday()
    {
        DifficultyScores difficultyScores = GetDifficultyScores();
        return difficultyScores != null ? difficultyScores.GetBestScoreToday() : 0;
    }

    public int GetBestScoreThisWeek()
    {
        DifficultyScores difficultyScores = GetDifficultyScores();
        return difficultyScores != null ? difficultyScores.GetBestScoreThisWeek() : 0;
    }

    public HighScoreData GetAllHighScores()
    {
        return highScoreData;
    }

    private void UpdatePanelScoreUI()
    {
        if (panelScoreCurrentScoreText != null)
        {
            panelScoreCurrentScoreText.text = playerScore.ToString();
        }

        if (panelScoreBestTodayText != null)
        {
            int bestToday = Mathf.Max(GetBestScoreToday(), playerScore);
            panelScoreBestTodayText.text = bestToday.ToString();
        }

        if (panelScoreBestWeekText != null)
        {
            int bestWeek = Mathf.Max(GetBestScoreThisWeek(), playerScore);
            panelScoreBestWeekText.text = bestWeek.ToString();
        }

        if (panelScoreBestAllTimeText != null)
        {
            int bestAllTime = Mathf.Max(GetBestScoreAllTime(), playerScore);
            panelScoreBestAllTimeText.text = bestAllTime.ToString();
        }

        if (panelScoreDifficultyText != null)
        {
            panelScoreDifficultyText.text = difficulty.ToString().ToUpper();

            Color difficultyColor = mediumDifficultyColor;
            switch (difficulty)
            {
                case Difficulty.Easy:
                    difficultyColor = easyDifficultyColor;
                    break;
                case Difficulty.Medium:
                    difficultyColor = mediumDifficultyColor;
                    break;
                case Difficulty.Hard:
                    difficultyColor = hardDifficultyColor;
                    break;
            }
            panelScoreDifficultyText.color = difficultyColor;
        }
    }

    public void OnHomeButton()
    {
        PlayClickSound();
        SceneManager.LoadScene(menuSceneName);
    }

    public void OnPlayAgainButton()
    {
        PlayClickSound();
        StartNewGame();
    }

    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
