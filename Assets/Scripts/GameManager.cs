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
    [SerializeField] private float timeBetweenButtons = 0.6f;
    [SerializeField] private float buttonPressDuration = 0.4f;
    [SerializeField] private float gameOverFadeDuration = 0.3f;

    [Header("Game State")]
    [SerializeField] private GameState currentState = GameState.Idle;
    [SerializeField] private List<ButtonColor> sequence = new List<ButtonColor>();
    [SerializeField] private int currentPlayerStep = 0;
    [SerializeField] private int currentRound = 0;
    [SerializeField] private int playerScore = 0;

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
        ButtonColor randomButton = (ButtonColor)Random.Range(0, 4);
        sequence.Add(randomButton);
    }

    private IEnumerator ShowSequenceCoroutine()
    {
        currentState = GameState.ShowingSequence;
        SetButtonsInteractable(false);

        yield return new WaitForSeconds(0.5f);

        foreach (ButtonColor button in sequence)
        {
            ActivateButton(button);
            yield return new WaitForSeconds(buttonPressDuration);
            DeactivateButton(button);
            yield return new WaitForSeconds(timeBetweenButtons - buttonPressDuration);
        }

        currentState = GameState.WaitingForPlayerInput;
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
        return buttonPressed == sequence[currentPlayerStep];
    }

    private void OnPlayerCorrect()
    {
        currentPlayerStep++;

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
