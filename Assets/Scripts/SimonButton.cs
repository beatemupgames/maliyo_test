using UnityEngine;

public class SimonButton : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private GameManager.ButtonColor buttonColor;
    [SerializeField] private float activeScale = 1.2f;
    [SerializeField] private float activeBrightness = 1.5f;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSound;

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

        if (audioSource != null && buttonSound != null)
        {
            audioSource.PlayOneShot(buttonSound);
        }
    }

    public void Deactivate()
    {
        SetNormalState();
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
