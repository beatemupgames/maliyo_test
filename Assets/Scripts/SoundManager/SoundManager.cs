using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    #region Nested Classes

    [System.Serializable]
    public class SoundClip
    {
        public string soundName;
        public AudioClip clip;
    }

    #endregion

    #region Singleton

    private static SoundManager instance;

    public static SoundManager Instance
    {
        get
        {
            return instance;
        }
    }

    #endregion

    #region Serialized Fields

    [Header("Sound Configuration")]
    [SerializeField] private List<SoundClip> soundClips = new List<SoundClip>();

    [Header("Audio Source Pool Settings")]
    [SerializeField] private int poolSize = 5;

    [Header("Volume Settings")]
    [SerializeField][Range(0f, 1f)] private float masterVolume = 1f;

    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogs = false;

    #endregion

    #region Private Fields

    private Dictionary<string, AudioClip> soundDictionary = new Dictionary<string, AudioClip>();
    private List<AudioSource> audioSourcePool = new List<AudioSource>();
    private GameObject audioSourceContainer;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Called when the script instance is loaded.
    /// Implements singleton pattern and initializes the sound manager.
    /// </summary>
    private void Awake()
    {
        // Implement singleton pattern with DontDestroyOnLoad
        if (instance == null)
        {
            // Set this as the singleton instance
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize the sound system
            InitializeSoundManager();
        }
        else if (instance != this)
        {
            // Destroy duplicate instances
            Destroy(gameObject);
        }
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes the sound manager by building the sound dictionary and creating the audio source pool.
    /// Called during Awake after singleton is established.
    /// </summary>
    private void InitializeSoundManager()
    {
        // Build sound dictionary from serialized sound clips list
        soundDictionary.Clear();
        foreach (SoundClip soundClip in soundClips)
        {
            // Validate sound clip data
            if (soundClip.clip != null && !string.IsNullOrEmpty(soundClip.soundName))
            {
                // Add to dictionary if not already present
                if (!soundDictionary.ContainsKey(soundClip.soundName))
                {
                    soundDictionary.Add(soundClip.soundName, soundClip.clip);
                }
                else
                {
                    // Warn about duplicate sound names
                    if (enableDebugLogs)
                    {
                        Debug.LogWarning($"Duplicate sound name found: {soundClip.soundName}. Skipping duplicate.");
                    }
                }
            }
        }

        // Create container GameObject to organize audio sources in hierarchy
        audioSourceContainer = new GameObject("AudioSourcePool");
        audioSourceContainer.transform.SetParent(transform);

        // Initialize audio source pool with specified pool size
        for (int i = 0; i < poolSize; i++)
        {
            CreateAudioSource();
        }

        // Log initialization success
        if (enableDebugLogs)
        {
            Debug.Log($"SoundManager initialized with {soundDictionary.Count} sounds and {poolSize} audio sources in pool.");
        }
    }

    #endregion

    #region Audio Source Pool Management

    /// <summary>
    /// Creates a new audio source and adds it to the pool.
    /// </summary>
    /// <returns>The newly created AudioSource component</returns>
    private AudioSource CreateAudioSource()
    {
        // Create GameObject for the audio source
        GameObject audioSourceObject = new GameObject($"AudioSource_{audioSourcePool.Count}");
        audioSourceObject.transform.SetParent(audioSourceContainer.transform);

        // Add and configure AudioSource component
        AudioSource audioSource = audioSourceObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = masterVolume;

        // Add to pool
        audioSourcePool.Add(audioSource);

        return audioSource;
    }

    /// <summary>
    /// Gets an available audio source from the pool.
    /// Creates a new one if all sources in the pool are busy.
    /// </summary>
    /// <returns>An available AudioSource component</returns>
    private AudioSource GetAvailableAudioSource()
    {
        // Search for an audio source that's not currently playing
        foreach (AudioSource audioSource in audioSourcePool)
        {
            if (!audioSource.isPlaying)
            {
                return audioSource;
            }
        }

        // All audio sources are busy, create a new temporary one
        if (enableDebugLogs)
        {
            Debug.Log("All audio sources in pool are busy. Creating temporary audio source.");
        }
        return CreateAudioSource();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Plays a sound by name using the master volume.
    /// Overload that uses the current master volume setting.
    /// </summary>
    /// <param name="soundName">The name of the sound to play</param>
    public void PlaySound(string soundName)
    {
        PlaySound(soundName, masterVolume);
    }

    /// <summary>
    /// Plays a sound by name with a specified volume.
    /// The volume is multiplied by the master volume.
    /// </summary>
    /// <param name="soundName">The name of the sound to play</param>
    /// <param name="volume">The volume to play the sound at (0-1)</param>
    public void PlaySound(string soundName, float volume)
    {
        // Validate sound name
        if (string.IsNullOrEmpty(soundName))
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning("Sound name is null or empty.");
            }
            return;
        }

        // Check if sound exists in dictionary
        if (!soundDictionary.ContainsKey(soundName))
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning($"Sound '{soundName}' not found in SoundManager.");
            }
            return;
        }

        // Get the audio clip
        AudioClip clip = soundDictionary[soundName];
        if (clip == null)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning($"AudioClip for sound '{soundName}' is null.");
            }
            return;
        }

        // Get an available audio source and play the sound
        AudioSource audioSource = GetAvailableAudioSource();
        if (audioSource != null)
        {
            audioSource.volume = volume * masterVolume;
            audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Sets the master volume for all sounds.
    /// Updates all audio sources in the pool to use the new volume.
    /// </summary>
    /// <param name="volume">The master volume (0-1), will be clamped</param>
    public void SetMasterVolume(float volume)
    {
        // Clamp volume to valid range
        masterVolume = Mathf.Clamp01(volume);

        // Update volume for all audio sources in pool
        foreach (AudioSource audioSource in audioSourcePool)
        {
            audioSource.volume = masterVolume;
        }
    }

    /// <summary>
    /// Gets the current master volume setting.
    /// </summary>
    /// <returns>The current master volume (0-1)</returns>
    public float GetMasterVolume()
    {
        return masterVolume;
    }

    /// <summary>
    /// Checks if a sound with the specified name exists in the sound manager.
    /// </summary>
    /// <param name="soundName">The name of the sound to check</param>
    /// <returns>True if the sound exists, false otherwise</returns>
    public bool HasSound(string soundName)
    {
        return soundDictionary.ContainsKey(soundName);
    }

    /// <summary>
    /// Stops all currently playing sounds.
    /// Iterates through all audio sources in the pool and stops playback.
    /// </summary>
    public void StopAllSounds()
    {
        // Stop all active audio sources
        foreach (AudioSource audioSource in audioSourcePool)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    #endregion
}
