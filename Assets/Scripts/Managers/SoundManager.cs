using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Represents an individual sound with its properties.
/// </summary>
[System.Serializable]
public class Sound
{
    [Tooltip("Unique name to identify the sound.")]
    public string name;           // Unique identifier for the sound

    [Tooltip("Audio clip to be played.")]
    public AudioClip clip;       // The audio clip

    [Tooltip("Volume level of the sound (0 to 1).")]
    [Range(0f, 1f)]
    public float volume = 1f;    // Volume level

    [Tooltip("Base pitch level of the sound (0.5 to 2).")]
    [Range(0.5f, 2f)]
    public float pitch = 1f;     // Base pitch level

    [Tooltip("Enable random pitch variation for this sound.")]
    public bool useRandomPitch = false; // Option to enable random pitch bending
}

/// <summary>
/// Manages and plays audio clips across the game.
/// </summary>
public class SoundManager : MonoBehaviour
{
    // Singleton instance
    public static SoundManager Instance { get; private set; }

    [Header("Sound Settings")]
    [Tooltip("List of sounds to manage. Assign via the Inspector.")]
    [SerializeField]
    private List<Sound> sounds = new List<Sound>(); // List of sounds assigned in the Inspector

    [Header("Pitch Randomness Settings")]
    [Tooltip("Maximum pitch variation range for sounds with random pitch enabled.")]
    [Range(0f, 0.5f)]
    [SerializeField]
    private float pitchRandomRange = 0.1f; // Global slider to control pitch randomness

    // Dictionary for quick sound lookup by name
    private Dictionary<string, Sound> soundDictionary;

    // Pool of AudioSources to handle multiple simultaneous sounds
    private List<AudioSource> audioSourcePool = new List<AudioSource>();

    /// <summary>
    /// Initializes the SoundManager singleton and prepares the sound dictionary.
    /// </summary>
    private void Awake()
    {
        // Implement Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
            InitializeSoundDictionary();
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

    /// <summary>
    /// Initializes the sound dictionary for efficient lookup.
    /// </summary>
    private void InitializeSoundDictionary()
    {
        soundDictionary = new Dictionary<string, Sound>();

        foreach (Sound s in sounds)
        {
            if (!soundDictionary.ContainsKey(s.name))
            {
                soundDictionary.Add(s.name, s);
            }
            else
            {
                Debug.LogWarning($"SoundManager: Duplicate sound name detected: '{s.name}'. Ignoring duplicate.");
            }
        }
    }

    /// <summary>
    /// Plays a sound by its unique name.
    /// Usage: SoundManager.PlaySound("AttackSound");
    /// </summary>
    /// <param name="soundName">The unique name of the sound to play.</param>
    public static void PlaySound(string soundName)
    {
        if (Instance == null)
        {
            Debug.LogError("SoundManager: Instance not found. Ensure a SoundManager is present in the scene.");
            return;
        }

        if (Instance.soundDictionary.TryGetValue(soundName, out Sound sound))
        {
            AudioSource source = Instance.GetAvailableAudioSource();
            source.clip = sound.clip;
            source.volume = sound.volume;

            // Apply random pitch if enabled
            if (sound.useRandomPitch)
            {
                float randomPitch = Random.Range(-Instance.pitchRandomRange, Instance.pitchRandomRange);
                source.pitch = Mathf.Clamp(sound.pitch + randomPitch, 0.5f, 2f); // Clamp to prevent extreme pitch values
            }
            else
            {
                source.pitch = sound.pitch;
            }

            source.Play();
        }
        else
        {
            Debug.LogError($"SoundManager: Sound '{soundName}' not found. Please check the sound name and ensure it is added to the SoundManager.");
        }
    }

    /// <summary>
    /// Retrieves an available AudioSource from the pool or creates a new one if necessary.
    /// </summary>
    /// <returns>An AudioSource ready to play a sound.</returns>
    private AudioSource GetAvailableAudioSource()
    {
        foreach (AudioSource source in audioSourcePool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        // If no available AudioSource, create a new one
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        audioSourcePool.Add(newSource);
        return newSource;
    }

    /// <summary>
    /// Optionally, you can add methods to dynamically add or remove sounds at runtime.
    /// </summary>
    // Example:
    /*
    public static void AddSound(string name, AudioClip clip, float volume = 1f, float pitch = 1f, bool useRandomPitch = false)
    {
        if (Instance == null)
        {
            Debug.LogError("SoundManager: Instance not found. Cannot add sound.");
            return;
        }

        if (!Instance.soundDictionary.ContainsKey(name))
        {
            Sound newSound = new Sound { name = name, clip = clip, volume = volume, pitch = pitch, useRandomPitch = useRandomPitch };
            Instance.sounds.Add(newSound);
            Instance.soundDictionary.Add(name, newSound);
        }
        else
        {
            Debug.LogWarning($"SoundManager: Sound '{name}' already exists. Cannot add duplicate.");
        }
    }
    */
}
