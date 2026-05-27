using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
/// <summary>
/// Управляет постоянным проигрывателем фоновой музыки, плейлистом и громкостью через аудио миксер
/// </summary>

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
    private int lastPlayedClipIndex = -1;
    private readonly List<int> shuffledClipOrder = new List<int>();
    private int shuffledOrderPosition;

    public static BackgroundMusicPlayer Instance { get; private set; }
    /// <summary>
    /// Инициализирует ссылки и внутреннее состояние до запуска сцены
    /// </summary>
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
    /// <summary>
    /// Каждый кадр проверяет ввод или обновляет визуальное состояние
    /// </summary>
    private void Update()
    {
        if (!loopPlaylist || musicClips == null || musicClips.Length <= 1 || audioSource == null || audioSource.isPlaying || audioSource.clip == null)
            return;

        PlayClip(GetNextClipIndex());
    }
    /// <summary>
    /// Освобождает подписки и временные ресурсы перед уничтожением объекта
    /// </summary>
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
    /// <summary>
    /// Запускает музыку, анимацию или другой визуальный процесс
    /// </summary>
    public void Play()
    {
        if (musicClips == null || musicClips.Length == 0)
            return;

        if (audioSource == null)
            ConfigureAudioSource();

        currentClipIndex = shufflePlaylist ? GetNextShuffledClipIndex() : Mathf.Clamp(currentClipIndex, 0, musicClips.Length - 1);
        PlayClip(currentClipIndex);
    }
    /// <summary>
    /// Настраивает ссылки и параметры, которые нужны компоненту для работы
    /// </summary>
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
    /// <summary>
    /// Запускает музыку, анимацию или другой визуальный процесс
    /// </summary>
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
        lastPlayedClipIndex = currentClipIndex;
    }
    /// <summary>
    /// Возвращает сохраненное или рассчитанное значение
    /// </summary>
    private int GetNextClipIndex()
    {
        if (musicClips == null || musicClips.Length <= 1)
            return 0;

        if (!shufflePlaylist)
            return (currentClipIndex + 1) % musicClips.Length;

        return GetNextShuffledClipIndex();
    }
    /// <summary>
    /// Достает следующий трек из перемешанного мешка плейлиста, чтобы треки не повторялись до конца мешка
    /// </summary>
    private int GetNextShuffledClipIndex()
    {
        if (musicClips == null || musicClips.Length == 0)
            return 0;

        if (musicClips.Length == 1)
            return 0;

        if (shuffledClipOrder.Count != musicClips.Length || shuffledOrderPosition >= shuffledClipOrder.Count)
            RebuildShuffledOrder();

        return shuffledClipOrder[shuffledOrderPosition++];
    }
    /// <summary>
    /// Собирает новый перемешанный мешок индексов треков для следующего прохода плейлиста
    /// </summary>
    private void RebuildShuffledOrder()
    {
        shuffledClipOrder.Clear();
        for (var i = 0; i < musicClips.Length; i++)
            shuffledClipOrder.Add(i);

        for (var i = shuffledClipOrder.Count - 1; i > 0; i--)
        {
            var swapIndex = Random.Range(0, i + 1);
            var current = shuffledClipOrder[i];
            shuffledClipOrder[i] = shuffledClipOrder[swapIndex];
            shuffledClipOrder[swapIndex] = current;
        }

        if (shuffledClipOrder.Count > 1 && shuffledClipOrder[0] == lastPlayedClipIndex)
        {
            var first = shuffledClipOrder[0];
            shuffledClipOrder[0] = shuffledClipOrder[1];
            shuffledClipOrder[1] = first;
        }

        shuffledOrderPosition = 0;
    }
    /// <summary>
    /// Читает сохраненную громкость музыки и применяет ее к параметру AudioMixer
    /// </summary>
    private void ApplySavedMixerVolume()
    {
        if (outputMixerGroup == null || outputMixerGroup.audioMixer == null)
            return;

        var musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(GameSettingsService.MusicVolumeKey, 0.5f));
        outputMixerGroup.audioMixer.SetFloat(GameSettingsService.MusicVolumeMixerParameter, GameSettingsService.GetMusicVolumeDecibels(musicVolume));
    }
}
