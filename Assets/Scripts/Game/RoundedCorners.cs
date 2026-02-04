using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class RoundedCorners : MonoBehaviour
{
    #region Serialized Fields

    [Header("Corner Settings")]
    [SerializeField] private float cornerRadius = 20f;

    #endregion

    #region Private Fields

    private Image image;
    private Material material;

    // Shader property identifiers cached for performance
    private static readonly string ShaderName = "UI/RoundedCorners";
    private static readonly int RadiusID = Shader.PropertyToID("_Radius");
    private static readonly int ResolutionID = Shader.PropertyToID("_Resolution");

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Called when the script instance is loaded.
    /// Initializes the Image component and creates a material with the rounded corners shader.
    /// </summary>
    private void Awake()
    {
        // Get the Image component attached to this GameObject
        image = GetComponent<Image>();

        // Find and load the rounded corners shader
        Shader shader = Shader.Find(ShaderName);
        if (shader == null)
        {
            // Warn if shader is not found and exit early
            Debug.LogWarning($"Shader '{ShaderName}' not found. Using default shader.");
            return;
        }

        // Create a new material instance with the shader
        material = new Material(shader);
        image.material = material;

        // Initialize material properties with current settings
        UpdateMaterial();
    }

    /// <summary>
    /// Called when the script is loaded or a value changes in the Inspector (editor only).
    /// Updates the material properties when values are modified in the editor.
    /// </summary>
    private void OnValidate()
    {
        // Only update during play mode to avoid issues in edit mode
        if (Application.isPlaying && material != null)
        {
            UpdateMaterial();
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Called when the RectTransform dimensions change (editor only).
    /// Updates the material to match the new size of the UI element.
    /// </summary>
    private void OnRectTransformDimensionsChange()
    {
        // Update material properties when dimensions change
        if (material != null)
        {
            UpdateMaterial();
        }
    }
#endif

    #endregion

    #region Private Methods

    /// <summary>
    /// Updates the shader material properties with the current corner radius and resolution.
    /// Called when the corner radius changes or when the RectTransform is resized.
    /// </summary>
    private void UpdateMaterial()
    {
        // Early exit if material is not initialized
        if (material == null) return;

        // Get the size of the RectTransform
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 size = rectTransform.rect.size;

        // Update shader properties with current values
        material.SetFloat(RadiusID, cornerRadius);
        material.SetVector(ResolutionID, new Vector4(size.x, size.y, 0, 0));
    }

    #endregion
}
