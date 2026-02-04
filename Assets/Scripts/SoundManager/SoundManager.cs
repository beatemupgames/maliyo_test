using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [System.Serializable]
    public class SoundClip
    {
        public string soundName;
        public AudioClip clip;
    }

    private static SoundManager instance;
    public static SoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("SoundManager instance is null! Make sure there's a SoundManager in the scene.");
            }
            return instance;
        }
    }

    [Header("Sound Configuration")]
    [SerializeField] private List<SoundClip> soundClips = new List<SoundClip>();

    [Header("Audio Source Pool Settings")]
    [SerializeField] private int poolSize = 5;

    [Header("Volume Settings")]
    [SerializeField][Range(0f, 1f)] private float masterVolume = 1f;

    private Dictionary<string, AudioClip> soundDictionary = new Dictionary<string, AudioClip>();
    private List<AudioSource> audioSourcePool = new List<AudioSource>();
    private GameObject audioSourceContainer;

    private void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSoundManager();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSoundManager()
    {
        // Create dictionary from sound clips list
        soundDictionary.Clear();
        foreach (SoundClip soundClip in soundClips)
        {
            if (soundClip.clip != null && !string.IsNullOrEmpty(soundClip.soundName))
            {
                if (!soundDictionary.ContainsKey(soundClip.soundName))
                {
                    soundDictionary.Add(soundClip.soundName, soundClip.clip);
                }
                else
                {
                    Debug.LogWarning($"Duplicate sound name found: {soundClip.soundName}. Skipping duplicate.");
                }
            }
        }

        // Create container for audio sources
        audioSourceContainer = new GameObject("AudioSourcePool");
        audioSourceContainer.transform.SetParent(transform);

        // Initialize audio source pool
        for (int i = 0; i < poolSize; i++)
        {
            CreateAudioSource();
        }

        Debug.Log($"SoundManager initialized with {soundDictionary.Count} sounds and {poolSize} audio sources in pool.");
    }

    private AudioSource CreateAudioSource()
    {
        GameObject audioSourceObject = new GameObject($"AudioSource_{audioSourcePool.Count}");
        audioSourceObject.transform.SetParent(audioSourceContainer.transform);

        AudioSource audioSource = audioSourceObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = masterVolume;

        audioSourcePool.Add(audioSource);

        return audioSource;
    }

    private AudioSource GetAvailableAudioSource()
    {
        // Find an audio source that's not currently playing
        foreach (AudioSource audioSource in audioSourcePool)
        {
            if (!audioSource.isPlaying)
            {
                return audioSource;
            }
        }

        // If all are busy, create a temporary one
        Debug.Log("All audio sources in pool are busy. Creating temporary audio source.");
        return CreateAudioSource();
    }

    public void PlaySound(string soundName)
    {
        PlaySound(soundName, masterVolume);
    }

    public void PlaySound(string soundName, float volume)
    {
        if (string.IsNullOrEmpty(soundName))
        {
            Debug.LogWarning("Sound name is null or empty.");
            return;
        }

        if (!soundDictionary.ContainsKey(soundName))
        {
            Debug.LogWarning($"Sound '{soundName}' not found in SoundManager.");
            return;
        }

        AudioClip clip = soundDictionary[soundName];
        if (clip == null)
        {
            Debug.LogWarning($"AudioClip for sound '{soundName}' is null.");
            return;
        }

        AudioSource audioSource = GetAvailableAudioSource();
        if (audioSource != null)
        {
            audioSource.volume = volume * masterVolume;
            audioSource.PlayOneShot(clip);
        }
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);

        // Update all audio sources in pool
        foreach (AudioSource audioSource in audioSourcePool)
        {
            audioSource.volume = masterVolume;
        }
    }

    public float GetMasterVolume()
    {
        return masterVolume;
    }

    public bool HasSound(string soundName)
    {
        return soundDictionary.ContainsKey(soundName);
    }

    public void StopAllSounds()
    {
        foreach (AudioSource audioSource in audioSourcePool)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}
