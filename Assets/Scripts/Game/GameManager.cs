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
    [SerializeField] private SimonButton[] easyButtons = new SimonButton[3];
    [SerializeField] private SimonButton[] normalButtons = new SimonButton[4];

    [Header("UI References")]
    [SerializeField] private TextMeshPro scoreNumberText2D;
    [SerializeField] private TextMeshPro scoreText2D;
    [SerializeField] private GameHUDManager gameHUDManager;
    [SerializeField] private PanelGameOverManager panelGameOverManager;
    [SerializeField] private PanelScoreManager panelScoreManager;

    [Header("Game Elements to Fade")]
    [SerializeField] private GameObject simonEasyGameObject;
    [SerializeField] private GameObject simonNormalGameObject;

    [Header("General Settings")]
    [SerializeField] private Difficulty difficulty = Difficulty.Medium;
    [SerializeField] private string menuSceneName = "Menu";

    [Header("Sequence & Button Timing")]
    [SerializeField] private float timeBetweenButtons = 0.6f;
    [SerializeField] private float buttonPressDuration = 0.4f;
    [SerializeField] private float simultaneousButtonDelay = 0.05f;

    [Header("Hard Mode")]
    [SerializeField] private float hardModeRotationSpeed = 30f;

    [Header("Score Animation")]
    [SerializeField] private Color scoreHighlightColor = Color.yellow;
    [SerializeField] private float scoreAnimationDuration = 0.3f;
    [SerializeField] private float scoreScaleMultiplier = 1.3f;

    [Header("Game Over Score Animation")]
    [SerializeField] private Color scoreGameOverColor = Color.red;
    [SerializeField] private float scoreGameOverTransitionDuration = 0.2f;
    [SerializeField] private float scoreGameOverHoldDuration = 0.6f;

    [Header("Game State")]
    [SerializeField] private GameState currentState = GameState.Idle;
    [SerializeField] private List<SequenceStep> sequence = new List<SequenceStep>();
    [SerializeField] private int currentPlayerStep = 0;
    [SerializeField] private int currentRound = 0;
    [SerializeField] private int playerScore = 0;

    private HashSet<ButtonColor> currentStepPressedButtons = new HashSet<ButtonColor>();
    private HighScoreData highScoreData = new HighScoreData();
    private string saveFilePath;
    private Color scoreNumberText2DOriginalColor;
    private Vector3 scoreNumberText2DOriginalScale;
    private Color scoreText2DOriginalColor;

    public GameState CurrentState => currentState;
    public int CurrentRound => currentRound;
    public int PlayerScore => playerScore;

    private SimonButton[] CurrentButtons => difficulty == Difficulty.Easy ? easyButtons : normalButtons;
    private GameObject CurrentSimonGameObject => difficulty == Difficulty.Easy ? simonEasyGameObject : simonNormalGameObject;

    private void Update()
    {
        // Rotate Simon GameObject in Hard mode
        if (difficulty == Difficulty.Hard && simonNormalGameObject != null && currentState != GameState.GameOver)
        {
            simonNormalGameObject.transform.Rotate(0f, 0f, hardModeRotationSpeed * Time.deltaTime);
        }
    }

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

        if (scoreNumberText2D != null)
        {
            scoreNumberText2DOriginalColor = scoreNumberText2D.color;
            scoreNumberText2DOriginalScale = scoreNumberText2D.transform.localScale;
        }

        if (scoreText2D != null)
        {
            scoreText2DOriginalColor = scoreText2D.color;
        }

        saveFilePath = Path.Combine(Application.persistentDataPath, "highscores.json");
        LoadHighScores();

        if (gameHUDManager != null)
        {
            gameHUDManager.Initialize(difficulty, GetBestScoreAllTime());
        }

        if (panelScoreManager != null)
        {
            panelScoreManager.UpdateDifficultyUI(difficulty);
        }

        // Activate/deactivate Simon GameObjects based on difficulty
        if (simonEasyGameObject != null)
        {
            simonEasyGameObject.SetActive(difficulty == Difficulty.Easy);
            simonEasyGameObject.transform.rotation = Quaternion.identity;
        }

        if (simonNormalGameObject != null)
        {
            simonNormalGameObject.SetActive(difficulty != Difficulty.Easy);
            simonNormalGameObject.transform.rotation = Quaternion.identity;
        }

        if (panelGameOverManager != null)
        {
            panelGameOverManager.gameObject.SetActive(false);
        }

        if (panelScoreManager != null)
        {
            panelScoreManager.gameObject.SetActive(false);
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

        if (panelGameOverManager != null)
        {
            panelGameOverManager.gameObject.SetActive(false);
        }

        if (panelScoreManager != null)
        {
            panelScoreManager.ResetPanel();
            panelScoreManager.gameObject.SetActive(false);
        }

        // Activate/deactivate Simon GameObjects based on difficulty
        if (simonEasyGameObject != null)
        {
            simonEasyGameObject.SetActive(difficulty == Difficulty.Easy);
            simonEasyGameObject.transform.rotation = Quaternion.identity;
        }

        if (simonNormalGameObject != null)
        {
            simonNormalGameObject.SetActive(difficulty != Difficulty.Easy);
            simonNormalGameObject.transform.rotation = Quaternion.identity;
        }

        RestoreGameElementsAlpha();
        UpdateScoreUI(false);
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
        if (panelGameOverManager != null)
        {
            panelGameOverManager.PlayGameOverSound();
        }

        // Start score text red animation
        StartCoroutine(ScoreTextRedFlashCoroutine());

        for (int i = 0; i < 2; i++)
        {
            foreach (SimonButton button in CurrentButtons)
            {
                if (button != null)
                {
                    button.ActivateGameOver();
                }
            }

            yield return new WaitForSeconds(0.3f);

            foreach (SimonButton button in CurrentButtons)
            {
                if (button != null)
                {
                    button.Deactivate();
                }
            }

            yield return new WaitForSeconds(0.3f);
        }

        if (panelGameOverManager != null)
        {
            panelGameOverManager.ShowPanel();
            yield return StartCoroutine(panelGameOverManager.FadeInCoroutine());
        }
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
        foreach (SimonButton button in CurrentButtons)
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
        foreach (SimonButton button in CurrentButtons)
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

    public void OnNoThanksButtonPressed()
    {
        if (currentState == GameState.GameOver)
        {
            StartCoroutine(NoThanksSequenceCoroutine());
        }
    }

    private IEnumerator NoThanksSequenceCoroutine()
    {
        if (panelGameOverManager != null)
        {
            yield return StartCoroutine(panelGameOverManager.FadeOutCoroutine());
            panelGameOverManager.HidePanel();
        }

        if (panelScoreManager != null)
        {
            yield return new WaitForSeconds(panelScoreManager.ScoreWaitDuration);

            // Activate panel first
            panelScoreManager.ShowPanel();

            int bestToday = Mathf.Max(GetBestScoreToday(), playerScore);
            int bestWeek = Mathf.Max(GetBestScoreThisWeek(), playerScore);
            int bestAllTime = Mathf.Max(GetBestScoreAllTime(), playerScore);

            panelScoreManager.UpdatePanelScoreUI(playerScore, bestToday, bestWeek, bestAllTime, difficulty);
            panelScoreManager.PlayPanelScoreSound();
            panelScoreManager.ActivateTriangles();

            StartCoroutine(FadeOutGameElementsCoroutine(panelScoreManager.ScoreFadeInDuration));
            panelScoreManager.StartTriangleAnimations();
            yield return StartCoroutine(panelScoreManager.FadeInPanelCoroutine(panelScoreManager.ScoreFadeInDuration));
        }
    }


    private IEnumerator FadeOutGameElementsCoroutine(float duration)
    {
        SpriteRenderer[] simonSpriteRenderers = null;
        Color[] spriteOriginalColors = null;
        TextMeshPro[] simonTextMeshPros = null;
        Color[] textOriginalColors = null;

        GameObject currentSimon = CurrentSimonGameObject;

        if (currentSimon != null)
        {
            simonSpriteRenderers = currentSimon.GetComponentsInChildren<SpriteRenderer>();
            if (simonSpriteRenderers != null && simonSpriteRenderers.Length > 0)
            {
                spriteOriginalColors = new Color[simonSpriteRenderers.Length];
                for (int i = 0; i < simonSpriteRenderers.Length; i++)
                {
                    spriteOriginalColors[i] = simonSpriteRenderers[i].color;
                }
            }

            simonTextMeshPros = currentSimon.GetComponentsInChildren<TextMeshPro>();
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
        GameObject currentSimon = CurrentSimonGameObject;

        if (currentSimon != null)
        {
            SpriteRenderer[] spriteRenderers = currentSimon.GetComponentsInChildren<SpriteRenderer>();
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

            TextMeshPro[] textMeshPros = currentSimon.GetComponentsInChildren<TextMeshPro>();
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


    private void UpdateScoreUI(bool animate = true)
    {
        string scoreString = playerScore.ToString();

        if (scoreNumberText2D != null)
        {
            scoreNumberText2D.text = scoreString;
            if (animate && playerScore > 0)
            {
                StartCoroutine(AnimateScoreTextCoroutine());
            }
        }

        if (gameHUDManager != null)
        {
            int bestScore = Mathf.Max(GetBestScoreAllTime(), playerScore);
            gameHUDManager.UpdateScores(playerScore, bestScore);
        }
    }

    private IEnumerator AnimateScoreTextCoroutine()
    {
        if (scoreNumberText2D == null) yield break;

        Transform textTransform = scoreNumberText2D.transform;
        float elapsedTime = 0f;
        float halfDuration = scoreAnimationDuration / 2f;

        // Scale up and change color
        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / halfDuration;

            textTransform.localScale = Vector3.Lerp(scoreNumberText2DOriginalScale, scoreNumberText2DOriginalScale * scoreScaleMultiplier, t);
            scoreNumberText2D.color = Color.Lerp(scoreNumberText2DOriginalColor, scoreHighlightColor, t);

            yield return null;
        }

        textTransform.localScale = scoreNumberText2DOriginalScale * scoreScaleMultiplier;
        scoreNumberText2D.color = scoreHighlightColor;

        elapsedTime = 0f;

        // Scale down and restore color
        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / halfDuration;

            textTransform.localScale = Vector3.Lerp(scoreNumberText2DOriginalScale * scoreScaleMultiplier, scoreNumberText2DOriginalScale, t);
            scoreNumberText2D.color = Color.Lerp(scoreHighlightColor, scoreNumberText2DOriginalColor, t);

            yield return null;
        }

        textTransform.localScale = scoreNumberText2DOriginalScale;
        scoreNumberText2D.color = scoreNumberText2DOriginalColor;
    }

    private IEnumerator ScoreTextRedFlashCoroutine()
    {
        float elapsedTime = 0f;

        // Fade to red
        while (elapsedTime < scoreGameOverTransitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / scoreGameOverTransitionDuration;

            if (scoreText2D != null)
            {
                scoreText2D.color = Color.Lerp(scoreText2DOriginalColor, scoreGameOverColor, t);
            }

            if (scoreNumberText2D != null)
            {
                scoreNumberText2D.color = Color.Lerp(scoreNumberText2DOriginalColor, scoreGameOverColor, t);
            }

            yield return null;
        }

        // Ensure full red
        if (scoreText2D != null)
        {
            scoreText2D.color = scoreGameOverColor;
        }

        if (scoreNumberText2D != null)
        {
            scoreNumberText2D.color = scoreGameOverColor;
        }

        // Hold red color
        yield return new WaitForSeconds(scoreGameOverHoldDuration);

        // Fade back to original colors
        elapsedTime = 0f;
        while (elapsedTime < scoreGameOverTransitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / scoreGameOverTransitionDuration;

            if (scoreText2D != null)
            {
                scoreText2D.color = Color.Lerp(scoreGameOverColor, scoreText2DOriginalColor, t);
            }

            if (scoreNumberText2D != null)
            {
                scoreNumberText2D.color = Color.Lerp(scoreGameOverColor, scoreNumberText2DOriginalColor, t);
            }

            yield return null;
        }

        // Ensure original colors are restored
        if (scoreText2D != null)
        {
            scoreText2D.color = scoreText2DOriginalColor;
        }

        if (scoreNumberText2D != null)
        {
            scoreNumberText2D.color = scoreNumberText2DOriginalColor;
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


    public void OnHomeButtonPressed()
    {
        SceneManager.LoadScene(menuSceneName);
    }

    public void OnPlayAgainButtonPressed()
    {
        StartNewGame();
    }

}
