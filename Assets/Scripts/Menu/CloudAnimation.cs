using UnityEngine;

/// <summary>
/// Controls the horizontal movement animation of clouds in the menu.
/// Clouds move smoothly from left to right following an animation curve.
/// </summary>
public class CloudAnimation : MonoBehaviour
{
    #region Variables

    [Header("Animation Settings")]
    [SerializeField] private float moveDistance = 2f; // Distance to move from center (-2 to +2)
    [SerializeField] private float animationDuration = 5f; // Duration for one complete cycle (back and forth)
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Smooth easing curve

    private Vector3 startPosition; // Initial position of the cloud
    private float elapsedTime = 0f; // Time elapsed since start

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Called when the script starts.
    /// Stores the initial position of the cloud to use as a reference point.
    /// </summary>
    private void Start()
    {
        // Store the initial position of the object
        startPosition = transform.position;
    }

    /// <summary>
    /// Called every frame.
    /// Updates the cloud position creating a smooth and continuous horizontal movement.
    /// </summary>
    private void Update()
    {
        // Increment elapsed time
        elapsedTime += Time.deltaTime;

        // Calculate normalized progress (0 to 1, repeating)
        float normalizedTime = (elapsedTime % animationDuration) / animationDuration;

        // Create ping-pong effect (0 -> 1 -> 0) for back and forth movement
        float pingPongValue = Mathf.PingPong(normalizedTime * 2f, 1f);

        // Apply easing curve for a more natural animation
        float easedValue = movementCurve.Evaluate(pingPongValue);

        // Calculate offset from center (-moveDistance to +moveDistance)
        float offset = Mathf.Lerp(-moveDistance, moveDistance, easedValue);

        // Apply horizontal movement to the cloud position
        transform.position = startPosition + new Vector3(offset, 0f, 0f);
    }

    #endregion
}
