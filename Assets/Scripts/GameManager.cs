using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

    [Header("Button References")]
    [SerializeField] private SimonButton[] buttons = new SimonButton[4];

    [Header("UI References")]
    [SerializeField] private TextMeshPro scoreText2D;
    [SerializeField] private TextMeshProUGUI scoreTextUI;
    [SerializeField] private GameObject panelGameOver;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip gameOverSound;

    [Header("Game Settings")]
    [SerializeField] private Difficulty difficulty = Difficulty.Medium;
    [SerializeField] private float timeBetweenButtons = 0.6f;
    [SerializeField] private float buttonPressDuration = 0.4f;
    [SerializeField] private float gameOverFadeDuration = 0.3f;
    [SerializeField] private float simultaneousButtonDelay = 0.05f;

    [Header("Game State")]
    [SerializeField] private GameState currentState = GameState.Idle;
    [SerializeField] private List<SequenceStep> sequence = new List<SequenceStep>();
    [SerializeField] private int currentPlayerStep = 0;
    [SerializeField] private int currentRound = 0;
    [SerializeField] private int playerScore = 0;

    private HashSet<ButtonColor> currentStepPressedButtons = new HashSet<ButtonColor>();

    public GameState CurrentState => currentState;
    public int CurrentRound => currentRound;
    public int PlayerScore => playerScore;

    private void Start()
    {
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(false);
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
                ButtonColor mediumButton = (ButtonColor)Random.Range(0, 4);
                newStep = new SequenceStep(mediumButton);
                break;

            case Difficulty.Hard:
                if (Random.value < 0.3f)
                {
                    List<ButtonColor> doubleButtons = new List<ButtonColor>();
                    ButtonColor first = (ButtonColor)Random.Range(0, 4);
                    ButtonColor second;
                    do
                    {
                        second = (ButtonColor)Random.Range(0, 4);
                    } while (second == first);

                    doubleButtons.Add(first);
                    doubleButtons.Add(second);
                    newStep = new SequenceStep(doubleButtons);
                }
                else
                {
                    ButtonColor hardButton = (ButtonColor)Random.Range(0, 4);
                    newStep = new SequenceStep(hardButton);
                }
                break;

            default:
                ButtonColor defaultButton = (ButtonColor)Random.Range(0, 4);
                newStep = new SequenceStep(defaultButton);
                break;
        }

        sequence.Add(newStep);
    }

    private ButtonColor GetRandomButtonForEasy()
    {
        int randomIndex = Random.Range(0, 3);
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
        CanvasGroup canvasGroup = panelGameOver.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = panelGameOver.AddComponent<CanvasGroup>();
        }

        float elapsedTime = 0f;
        canvasGroup.alpha = 0f;

        while (elapsedTime < gameOverFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / gameOverFadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
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
    }
}
