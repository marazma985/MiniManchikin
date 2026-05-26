using UnityEngine;
using UnityEngine.Audio;

[DisallowMultipleComponent]
public sealed class BackgroundMusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip[] musicClips;
    [SerializeField] private AudioMixerGroup outputMixerGroup;
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private bool loopPlaylist = true;
    [SerializeField] private bool shufflePlaylist;

    private AudioSource audioSource;
    private int currentClipIndex;

    public static BackgroundMusicPlayer Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ConfigureAudioSource();
        ApplySavedMixerVolume();

        if (playOnAwake)
            Play();
    }

    private void Update()
    {
        if (!loopPlaylist || musicClips == null || musicClips.Length <= 1 || audioSource == null || audioSource.isPlaying || audioSource.clip == null)
            return;

        PlayClip(GetNextClipIndex());
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Play()
    {
        if (musicClips == null || musicClips.Length == 0)
            return;

        if (audioSource == null)
            ConfigureAudioSource();

        currentClipIndex = Mathf.Clamp(currentClipIndex, 0, musicClips.Length - 1);
        PlayClip(currentClipIndex);
    }

    private void ConfigureAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = musicClips == null || musicClips.Length <= 1;
        audioSource.spatialBlend = 0f;
        audioSource.outputAudioMixerGroup = outputMixerGroup;
    }

    private void PlayClip(int clipIndex)
    {
        if (audioSource == null || musicClips == null || musicClips.Length == 0)
            return;

        currentClipIndex = Mathf.Clamp(clipIndex, 0, musicClips.Length - 1);
        var clip = musicClips[currentClipIndex];
        if (clip == null)
            return;

        audioSource.clip = clip;
        audioSource.loop = musicClips.Length <= 1;
        audioSource.Play();
    }

    private int GetNextClipIndex()
    {
        if (musicClips == null || musicClips.Length <= 1)
            return 0;

        if (!shufflePlaylist)
            return (currentClipIndex + 1) % musicClips.Length;

        var nextIndex = Random.Range(0, musicClips.Length);
        if (nextIndex == currentClipIndex)
            nextIndex = (nextIndex + 1) % musicClips.Length;

        return nextIndex;
    }

    private void ApplySavedMixerVolume()
    {
        if (outputMixerGroup == null || outputMixerGroup.audioMixer == null)
            return;

        var musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(GameSettingsService.MusicVolumeKey, 0.5f));
        outputMixerGroup.audioMixer.SetFloat(GameSettingsService.MusicVolumeMixerParameter, GameSettingsService.GetMusicVolumeDecibels(musicVolume));
    }
}
