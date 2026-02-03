using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class RoundedCorners : MonoBehaviour
{
    [SerializeField] private float cornerRadius = 20f;

    private Image image;
    private Material material;

    private static readonly string ShaderName = "UI/RoundedCorners";
    private static readonly int RadiusID = Shader.PropertyToID("_Radius");
    private static readonly int ResolutionID = Shader.PropertyToID("_Resolution");

    private void Awake()
    {
        image = GetComponent<Image>();

        // Create shader if it doesn't exist
        Shader shader = Shader.Find(ShaderName);
        if (shader == null)
        {
            Debug.LogWarning($"Shader '{ShaderName}' not found. Using default shader.");
            return;
        }

        material = new Material(shader);
        image.material = material;

        UpdateMaterial();
    }

    private void OnValidate()
    {
        if (Application.isPlaying && material != null)
        {
            UpdateMaterial();
        }
    }

    private void UpdateMaterial()
    {
        if (material == null) return;

        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 size = rectTransform.rect.size;

        material.SetFloat(RadiusID, cornerRadius);
        material.SetVector(ResolutionID, new Vector4(size.x, size.y, 0, 0));
    }

#if UNITY_EDITOR
    private void OnRectTransformDimensionsChange()
    {
        if (material != null)
        {
            UpdateMaterial();
        }
    }
#endif
}
