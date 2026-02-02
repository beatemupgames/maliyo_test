using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Game Settings")]
    [SerializeField] private float timeBetweenButtons = 0.6f;
    [SerializeField] private float buttonPressDuration = 0.4f;

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
        StartNewGame();
    }

    public void StartNewGame()
    {
        sequence.Clear();
        currentPlayerStep = 0;
        currentRound = 0;
        playerScore = 0;
        currentState = GameState.Idle;

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

        yield return new WaitForSeconds(0.5f);

        foreach (ButtonColor button in sequence)
        {
            ActivateButton(button);
            yield return new WaitForSeconds(buttonPressDuration);
            DeactivateButton(button);
            yield return new WaitForSeconds(timeBetweenButtons - buttonPressDuration);
        }

        currentState = GameState.WaitingForPlayerInput;
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
        playerScore += 10;

        if (currentPlayerStep >= sequence.Count)
        {
            playerScore += currentRound * 5;
            StartNextRound();
        }
    }

    private void OnPlayerFail()
    {
        currentState = GameState.GameOver;
        Debug.Log($"Game Over! Final Score: {playerScore}, Rounds Completed: {currentRound - 1}");
    }

    private void ActivateButton(ButtonColor button)
    {
        Debug.Log($"Activating button: {button}");
        // TODO: Trigger visual and audio feedback for the button
    }

    private void DeactivateButton(ButtonColor button)
    {
        Debug.Log($"Deactivating button: {button}");
        // TODO: Stop visual and audio feedback for the button
    }

    public void RestartGame()
    {
        if (currentState == GameState.GameOver)
        {
            StartNewGame();
        }
    }
}
