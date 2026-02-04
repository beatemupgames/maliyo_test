using UnityEngine;

public class SimonButton : MonoBehaviour
{
    #region Serialized Fields

    [Header("Button Settings")]
    [SerializeField] private GameManager.ButtonColor buttonColor;
    [SerializeField] private float activeScale = 1.2f;
    [SerializeField] private float activeBrightness = 1.5f;
    [SerializeField] private float gameOverBrightness = 3.0f;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    #endregion

    #region Private Fields

    private Vector3 originalScale;
    private Color originalColor;
    private Collider2D buttonCollider;

    #endregion

    #region Properties

    public GameManager.ButtonColor ButtonColor => buttonColor;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Called when the script instance is loaded.
    /// Initializes button references and stores original scale and color.
    /// </summary>
    private void Awake()
    {
        // Store original scale for animations
        originalScale = transform.localScale;

        // Get or find sprite renderer component
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Store original color for brightness modifications
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Cache collider component for interactability control
        buttonCollider = GetComponent<Collider2D>();
    }

    /// <summary>
    /// Called before the first frame update.
    /// Sets the button to its normal initial state.
    /// </summary>
    private void Start()
    {
        // Initialize button to normal state
        SetNormalState();
    }

    /// <summary>
    /// Called when the mouse button is pressed on this button's collider.
    /// Notifies the GameManager if the game is waiting for player input.
    /// </summary>
    private void OnMouseDown()
    {
        // Find GameManager in the scene
        GameManager gameManager = FindFirstObjectByType<GameManager>();

        // Only process input if game is waiting for player
        if (gameManager != null && gameManager.CurrentState == GameManager.GameState.WaitingForPlayerInput)
        {
            // Notify GameManager of button press
            gameManager.OnPlayerPressButton(buttonColor);
        }
    }

    #endregion

    #region Button State Management

    /// <summary>
    /// Activates the button with normal brightness.
    /// Scales up the button and plays the corresponding sound effect.
    /// Used during sequence display and correct player input.
    /// </summary>
    public void Activate()
    {
        // Increase brightness for active state
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor * activeBrightness;
        }

        // Scale up for visual feedback
        transform.localScale = originalScale * activeScale;

        // Play button-specific sound effect
        if (SoundManager.Instance != null)
        {
            string soundName = $"{buttonColor}Button";
            SoundManager.Instance.PlaySound(soundName);
        }
    }

    /// <summary>
    /// Deactivates the button and returns it to normal state.
    /// Restores original scale and color.
    /// </summary>
    public void Deactivate()
    {
        // Return to normal state
        SetNormalState();
    }

    /// <summary>
    /// Activates the button with game over brightness (extra bright).
    /// Used during the game over flash animation to indicate failure.
    /// </summary>
    public void ActivateGameOver()
    {
        // Use extra bright color for game over state
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor * gameOverBrightness;
        }

        // Scale up for visual feedback
        transform.localScale = originalScale * activeScale;
    }

    /// <summary>
    /// Sets the button to its normal state.
    /// Restores original scale and color.
    /// </summary>
    private void SetNormalState()
    {
        // Restore original color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        // Restore original scale
        transform.localScale = originalScale;
    }

    #endregion

    #region Interactability

    /// <summary>
    /// Sets whether the button can be clicked by the player.
    /// Enables or disables the collider to control interactability.
    /// </summary>
    /// <param name="interactable">True to enable clicks, false to disable</param>
    public void SetInteractable(bool interactable)
    {
        // Enable or disable collider to control clickability
        if (buttonCollider != null)
        {
            buttonCollider.enabled = interactable;
        }
    }

    #endregion
}
