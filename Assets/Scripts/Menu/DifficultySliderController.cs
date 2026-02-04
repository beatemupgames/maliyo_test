using UnityEngine;
using UnityEngine.UI;

public class DifficultySliderController : MonoBehaviour
{
    #region Serialized Fields

    [Header("Slider Components")]
    [SerializeField] private Slider difficultySlider;
    [SerializeField] private RectTransform sliderHandle;
    [SerializeField] private Image handleInsideImage;

    [Header("Dependencies")]
    [SerializeField] private DifficultyColorProvider colorProvider;

    [Header("Animation Settings")]
    [SerializeField] private float sliderSnapSpeed = 10f;
    [SerializeField] private float handlePulseScale = 1.1f;
    [SerializeField] private float handlePulseDuration = 1.5f;

    #endregion

    #region Events

    public System.Action<float, Color> OnSliderValueChanged;
    public System.Action<int> OnDifficultySnapped;

    #endregion

    #region Private Fields

    private bool isDraggingSlider = false;
    private float targetSliderValue;
    private bool isSnapping = false;
    private bool isSliderBeingDragged = false;
    private float handlePulseTimer = 0f;
    private Vector3 handleOriginalScale;

    #endregion

    #region Properties

    public float SliderValue => difficultySlider != null ? difficultySlider.value : 0f;

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes the difficulty slider with the specified initial difficulty value.
    /// Sets up slider properties, event listeners, and UI components.
    /// </summary>
    /// <param name="initialDifficulty">The initial difficulty level (0-2)</param>
    public void Initialize(int initialDifficulty)
    {
        // Setup slider configuration and event listeners
        if (difficultySlider != null)
        {
            // Configure slider range and initial value
            difficultySlider.minValue = 0;
            difficultySlider.maxValue = 2;
            difficultySlider.wholeNumbers = false;
            difficultySlider.value = initialDifficulty;
            targetSliderValue = initialDifficulty;
            difficultySlider.onValueChanged.AddListener(OnSliderChanged);

            // Setup event trigger for pointer interactions
            UnityEngine.EventSystems.EventTrigger trigger = difficultySlider.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (trigger == null)
            {
                trigger = difficultySlider.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }

            // Register pointer down event to detect when user starts dragging
            UnityEngine.EventSystems.EventTrigger.Entry pointerDownEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerDownEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
            pointerDownEntry.callback.AddListener((data) => { OnSliderPointerDown(); });
            trigger.triggers.Add(pointerDownEntry);

            // Register pointer up event to detect when user releases the slider
            UnityEngine.EventSystems.EventTrigger.Entry pointerUpEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerUpEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
            pointerUpEntry.callback.AddListener((data) => { OnSliderPointerUp(); });
            trigger.triggers.Add(pointerUpEntry);

            // Store original scale for pulse animation
            if (sliderHandle != null)
            {
                handleOriginalScale = sliderHandle.localScale;
            }
        }

        // Initialize visual state
        UpdateHandleColor();

        // Notify listeners of initial state
        Color initialColor = GetCurrentDifficultyColor();
        OnSliderValueChanged?.Invoke(difficultySlider.value, initialColor);
    }

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Called every frame to handle slider snapping animation and handle pulse effect.
    /// </summary>
    private void Update()
    {
        // Smoothly snap slider to target value when not being dragged
        if (difficultySlider != null && isSnapping && !isDraggingSlider)
        {
            // Lerp slider value towards target for smooth transition
            difficultySlider.value = Mathf.Lerp(difficultySlider.value, targetSliderValue, Time.deltaTime * sliderSnapSpeed);

            // Update handle color to match current value during animation
            UpdateHandleColor();

            // Notify listeners of value change
            Color currentColor = GetCurrentDifficultyColor();
            OnSliderValueChanged?.Invoke(difficultySlider.value, currentColor);

            // Stop snapping when close enough to target value
            if (Mathf.Abs(difficultySlider.value - targetSliderValue) < 0.01f)
            {
                difficultySlider.value = targetSliderValue;
                isSnapping = false;
            }
        }

        // Animate handle pulse effect when idle
        if (!isSliderBeingDragged)
        {
            // Update pulse timer
            handlePulseTimer += Time.deltaTime;
            float normalizedTime = (handlePulseTimer % handlePulseDuration) / handlePulseDuration;

            // Calculate pulse scale using sine wave for smooth oscillation
            float pulseValue = Mathf.Sin(normalizedTime * Mathf.PI);
            float scale = Mathf.Lerp(1f, handlePulseScale, pulseValue);

            // Apply scale to handle (child HandleInside inherits the scale)
            if (sliderHandle != null)
            {
                sliderHandle.localScale = handleOriginalScale * scale;
            }
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Called when the user starts dragging the slider.
    /// Disables snapping and pulse animation while dragging.
    /// </summary>
    private void OnSliderPointerDown()
    {
        // Set dragging flags to prevent animations during user interaction
        isDraggingSlider = true;
        isSnapping = false;
        isSliderBeingDragged = true;
    }

    /// <summary>
    /// Called when the user releases the slider.
    /// Initiates snapping to the nearest difficulty value.
    /// </summary>
    private void OnSliderPointerUp()
    {
        // Clear dragging flags
        isDraggingSlider = false;
        isSliderBeingDragged = false;

        // Snap to nearest difficulty value
        if (difficultySlider != null)
        {
            // Round to nearest integer difficulty level (0, 1, or 2)
            targetSliderValue = Mathf.Round(difficultySlider.value);
            isSnapping = true;

            // Notify listeners that difficulty has been selected
            OnDifficultySnapped?.Invoke((int)targetSliderValue);
        }
    }

    /// <summary>
    /// Called when the slider value changes (either by user or programmatically).
    /// Updates the handle color and notifies listeners.
    /// </summary>
    /// <param name="value">The new slider value</param>
    private void OnSliderChanged(float value)
    {
        // Update handle color to reflect current difficulty
        UpdateHandleColor();

        // Notify listeners of value and color change
        Color currentColor = GetCurrentDifficultyColor();
        OnSliderValueChanged?.Invoke(value, currentColor);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Gets the color corresponding to the current difficulty slider value.
    /// Uses the color provider to interpolate between difficulty colors.
    /// </summary>
    /// <returns>The color for the current slider value, or white if components are missing</returns>
    public Color GetCurrentDifficultyColor()
    {
        // Return white as fallback if dependencies are missing
        if (colorProvider == null || difficultySlider == null)
            return Color.white;

        // Get interpolated color from color provider
        return colorProvider.GetColorForSliderValue(difficultySlider.value);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Updates the handle's inside image color to match the current difficulty.
    /// Called whenever the slider value changes.
    /// </summary>
    private void UpdateHandleColor()
    {
        // Early exit if slider is not assigned
        if (difficultySlider == null)
            return;

        // Get the color for current difficulty level
        Color targetColor = GetCurrentDifficultyColor();

        // Apply color to the handle's inside image
        if (handleInsideImage != null)
        {
            handleInsideImage.color = targetColor;
        }
    }

    #endregion
}
