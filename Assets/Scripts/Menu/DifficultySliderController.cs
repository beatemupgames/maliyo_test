using UnityEngine;
using UnityEngine.UI;

public class DifficultySliderController : MonoBehaviour
{
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

    // Events
    public System.Action<float, Color> OnSliderValueChanged;
    public System.Action<int> OnDifficultySnapped;

    private bool isDraggingSlider = false;
    private float targetSliderValue;
    private bool isSnapping = false;
    private bool isSliderBeingDragged = false;
    private float handlePulseTimer = 0f;
    private Vector3 handleOriginalScale;

    public float SliderValue => difficultySlider != null ? difficultySlider.value : 0f;

    public void Initialize(int initialDifficulty)
    {
        // Setup slider
        if (difficultySlider != null)
        {
            difficultySlider.minValue = 0;
            difficultySlider.maxValue = 2;
            difficultySlider.wholeNumbers = false;
            difficultySlider.value = initialDifficulty;
            targetSliderValue = initialDifficulty;
            difficultySlider.onValueChanged.AddListener(OnSliderChanged);

            // Add event triggers for pointer down/up
            UnityEngine.EventSystems.EventTrigger trigger = difficultySlider.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (trigger == null)
            {
                trigger = difficultySlider.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }

            // Pointer down event
            UnityEngine.EventSystems.EventTrigger.Entry pointerDownEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerDownEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
            pointerDownEntry.callback.AddListener((data) => { OnSliderPointerDown(); });
            trigger.triggers.Add(pointerDownEntry);

            // Pointer up event
            UnityEngine.EventSystems.EventTrigger.Entry pointerUpEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerUpEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
            pointerUpEntry.callback.AddListener((data) => { OnSliderPointerUp(); });
            trigger.triggers.Add(pointerUpEntry);

            // Store original scales for pulse animation
            if (sliderHandle != null)
            {
                handleOriginalScale = sliderHandle.localScale;
            }
        }

        // Initialize UI
        UpdateHandleColor();

        // Notify initial state
        Color initialColor = GetCurrentDifficultyColor();
        OnSliderValueChanged?.Invoke(difficultySlider.value, initialColor);
    }

    private void Update()
    {
        // Smoothly snap to target value when not dragging
        if (difficultySlider != null && isSnapping && !isDraggingSlider)
        {
            difficultySlider.value = Mathf.Lerp(difficultySlider.value, targetSliderValue, Time.deltaTime * sliderSnapSpeed);

            // Update handle color during snapping animation
            UpdateHandleColor();

            // Notify listeners
            Color currentColor = GetCurrentDifficultyColor();
            OnSliderValueChanged?.Invoke(difficultySlider.value, currentColor);

            // Stop snapping when close enough
            if (Mathf.Abs(difficultySlider.value - targetSliderValue) < 0.01f)
            {
                difficultySlider.value = targetSliderValue;
                isSnapping = false;
            }
        }

        // Animate handle pulse when user is not dragging
        if (!isSliderBeingDragged)
        {
            handlePulseTimer += Time.deltaTime;
            float normalizedTime = (handlePulseTimer % handlePulseDuration) / handlePulseDuration;

            // Create a smooth pulse using sine wave (0 -> 1 -> 0)
            float pulseValue = Mathf.Sin(normalizedTime * Mathf.PI);
            float scale = Mathf.Lerp(1f, handlePulseScale, pulseValue);

            // Animate Handle only (HandleInside will inherit the scale as a child)
            if (sliderHandle != null)
            {
                sliderHandle.localScale = handleOriginalScale * scale;
            }
        }
    }

    private void OnSliderPointerDown()
    {
        isDraggingSlider = true;
        isSnapping = false;
        isSliderBeingDragged = true;
    }

    private void OnSliderPointerUp()
    {
        isDraggingSlider = false;
        isSliderBeingDragged = false;

        // Snap to nearest value
        if (difficultySlider != null)
        {
            targetSliderValue = Mathf.Round(difficultySlider.value);
            isSnapping = true;

            // Notify that slider has snapped to a difficulty value
            OnDifficultySnapped?.Invoke((int)targetSliderValue);
        }
    }

    private void OnSliderChanged(float value)
    {
        // Update handle color in real-time
        UpdateHandleColor();

        // Notify listeners
        Color currentColor = GetCurrentDifficultyColor();
        OnSliderValueChanged?.Invoke(value, currentColor);
    }

    public Color GetCurrentDifficultyColor()
    {
        if (colorProvider == null || difficultySlider == null)
            return Color.white;

        return colorProvider.GetColorForSliderValue(difficultySlider.value);
    }

    private void UpdateHandleColor()
    {
        if (difficultySlider == null)
            return;

        Color targetColor = GetCurrentDifficultyColor();

        // Update HandleInside color
        if (handleInsideImage != null)
        {
            handleInsideImage.color = targetColor;
        }
    }
}
