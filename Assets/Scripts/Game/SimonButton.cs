using UnityEngine;

public class SimonButton : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private GameManager.ButtonColor buttonColor;
    [SerializeField] private float activeScale = 1.2f;
    [SerializeField] private float activeBrightness = 1.5f;
    [SerializeField] private float gameOverBrightness = 3.0f;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Vector3 originalScale;
    private Color originalColor;

    public GameManager.ButtonColor ButtonColor => buttonColor;

    private void Awake()
    {
        originalScale = transform.localScale;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void Start()
    {
        SetNormalState();
    }

    private void OnMouseDown()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null && gameManager.CurrentState == GameManager.GameState.WaitingForPlayerInput)
        {
            gameManager.OnPlayerPressButton(buttonColor);
        }
    }

    public void Activate()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor * activeBrightness;
        }

        transform.localScale = originalScale * activeScale;

        // Play sound based on button color
        if (SoundManager.Instance != null)
        {
            string soundName = $"{buttonColor}Button";
            SoundManager.Instance.PlaySound(soundName);
        }
    }

    public void Deactivate()
    {
        SetNormalState();
    }

    public void ActivateGameOver()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor * gameOverBrightness;
        }

        transform.localScale = originalScale * activeScale;
    }

    private void SetNormalState()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        transform.localScale = originalScale;
    }

    public void SetInteractable(bool interactable)
    {
        enabled = interactable;
    }
}
