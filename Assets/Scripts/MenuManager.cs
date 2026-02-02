using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public enum Difficulty
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("Difficulty Settings")]
    [SerializeField] private Slider difficultySlider;
    [SerializeField] private TextMeshProUGUI difficultyText;
    [SerializeField] private Image difficultyIcon;
    [SerializeField] private Sprite easyDifficultySprite;
    [SerializeField] private Sprite mediumDifficultySprite;
    [SerializeField] private Sprite hardDifficultySprite;
    [SerializeField] private Color easyDifficultyColor = new Color(0.184f, 0.718f, 0.255f); // #2FB741
    [SerializeField] private Color mediumDifficultyColor = new Color(0.992f, 0.655f, 0.016f); // #FDA704
    [SerializeField] private Color hardDifficultyColor = new Color(0.996f, 0.373f, 0.337f); // #FE5F56

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;

    private Difficulty currentDifficulty = Difficulty.Easy;

    private void Start()
    {
        // Load saved difficulty or default to Easy
        if (PlayerPrefs.HasKey("GameDifficulty"))
        {
            string savedDifficulty = PlayerPrefs.GetString("GameDifficulty");
            if (System.Enum.TryParse(savedDifficulty, true, out Difficulty loadedDifficulty))
            {
                currentDifficulty = loadedDifficulty;
            }
        }

        // Setup slider
        if (difficultySlider != null)
        {
            difficultySlider.minValue = 0;
            difficultySlider.maxValue = 2;
            difficultySlider.wholeNumbers = true;
            difficultySlider.value = (int)currentDifficulty;
            difficultySlider.onValueChanged.AddListener(OnDifficultySliderChanged);
        }

        // Update UI to reflect current difficulty
        UpdateDifficultyUI();
    }

    private void OnDifficultySliderChanged(float value)
    {
        currentDifficulty = (Difficulty)(int)value;
        UpdateDifficultyUI();
        PlayClickSound();
    }

    private void UpdateDifficultyUI()
    {
        string difficultyName = "";
        Color difficultyColor = mediumDifficultyColor;
        Sprite difficultySprite = mediumDifficultySprite;

        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                difficultyName = "EASY";
                difficultyColor = easyDifficultyColor;
                difficultySprite = easyDifficultySprite;
                break;
            case Difficulty.Medium:
                difficultyName = "MEDIUM";
                difficultyColor = mediumDifficultyColor;
                difficultySprite = mediumDifficultySprite;
                break;
            case Difficulty.Hard:
                difficultyName = "HARD";
                difficultyColor = hardDifficultyColor;
                difficultySprite = hardDifficultySprite;
                break;
        }

        if (difficultyText != null)
        {
            difficultyText.text = difficultyName;
            difficultyText.color = difficultyColor;
        }

        if (difficultyIcon != null && difficultySprite != null)
        {
            difficultyIcon.sprite = difficultySprite;
        }
    }

    public void OnPlayButton()
    {
        PlayClickSound();

        // Save the selected difficulty
        PlayerPrefs.SetString("GameDifficulty", currentDifficulty.ToString());
        PlayerPrefs.Save();

        // Load the game scene
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnQuitButton()
    {
        PlayClickSound();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
