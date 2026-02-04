using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyColorsConfig", menuName = "Game/Difficulty Colors Config")]
public class DifficultyColorsConfig : ScriptableObject
{
    [Header("Difficulty Colors")]
    [SerializeField] private Color easyDifficultyColor = new Color(0.184f, 0.718f, 0.255f); // #2FB741
    [SerializeField] private Color mediumDifficultyColor = new Color(0.992f, 0.655f, 0.016f); // #FDA704
    [SerializeField] private Color hardDifficultyColor = new Color(0.996f, 0.373f, 0.337f); // #FE5F56

    public Color EasyColor => easyDifficultyColor;
    public Color MediumColor => mediumDifficultyColor;
    public Color HardColor => hardDifficultyColor;

    /// <summary>
    /// Gets the color for a specific difficulty index (0 = Easy, 1 = Medium, 2 = Hard)
    /// </summary>
    public Color GetColorForDifficulty(int difficultyIndex)
    {
        switch (difficultyIndex)
        {
            case 0: return easyDifficultyColor;
            case 1: return mediumDifficultyColor;
            case 2: return hardDifficultyColor;
            default: return easyDifficultyColor;
        }
    }
}
