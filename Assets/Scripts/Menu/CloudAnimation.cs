using UnityEngine;

public class CloudAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float moveDistance = 2f; // Distance to move from center (-2 to +2)
    [SerializeField] private float animationDuration = 5f; // Duration for one complete cycle (back and forth)
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Smooth easing curve

    private Vector3 startPosition;
    private float elapsedTime = 0f;

    private void Start()
    {
        // Store the initial position
        startPosition = transform.position;
    }

    private void Update()
    {
        // Update elapsed time
        elapsedTime += Time.deltaTime;

        // Calculate progress (0 to 1, then back to 0, repeating)
        float normalizedTime = (elapsedTime % animationDuration) / animationDuration;

        // Create a ping-pong effect (0 -> 1 -> 0)
        float pingPongValue = Mathf.PingPong(normalizedTime * 2f, 1f);

        // Apply easing curve for smooth animation
        float easedValue = movementCurve.Evaluate(pingPongValue);

        // Calculate offset from center (-moveDistance to +moveDistance)
        float offset = Mathf.Lerp(-moveDistance, moveDistance, easedValue);

        // Apply the horizontal movement
        transform.position = startPosition + new Vector3(offset, 0f, 0f);
    }
}
