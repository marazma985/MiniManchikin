using UnityEngine;
using UnityEngine.Audio;

public sealed class GameSettingsService : MonoBehaviour
{
    public const string ResolutionIndexKey = "Settings.ResolutionIndex";
    public const string FullscreenKey = "Settings.Fullscreen";
    public const string MusicVolumeKey = "Settings.MusicVolume";
    public const string MusicVolumeMixerParameter = "MusicVolume";
    private const float MaxMusicVolumeMultiplier = 0.5f;

    private static readonly Vector2Int[] WindowSizes =
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080)
    };

    [SerializeField] private AudioMixer mainAudioMixer;

    public int ResolutionCount => WindowSizes.Length;
    public AudioMixer MainAudioMixer => mainAudioMixer;

    private void Start()
    {
        ApplySavedSettings();
    }

    public string GetResolutionLabel(int index)
    {
        var size = GetWindowSize(index);
        return $"{size.x}x{size.y}";
    }

    public int GetSavedResolutionIndex()
    {
        return Mathf.Clamp(PlayerPrefs.GetInt(ResolutionIndexKey, 1), 0, WindowSizes.Length - 1);
    }

    public bool GetSavedFullscreen()
    {
        return PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
    }

    public float GetSavedMusicVolume()
    {
        return Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, 0.5f));
    }

    public void SaveAndApply(int resolutionIndex, bool fullscreen, float musicVolume)
    {
        var clampedResolutionIndex = Mathf.Clamp(resolutionIndex, 0, WindowSizes.Length - 1);
        var clampedMusicVolume = Mathf.Clamp01(musicVolume);

        PlayerPrefs.SetInt(ResolutionIndexKey, clampedResolutionIndex);
        PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
        PlayerPrefs.SetFloat(MusicVolumeKey, clampedMusicVolume);
        PlayerPrefs.Save();

        ApplySettings(clampedResolutionIndex, fullscreen, clampedMusicVolume);
    }

    public void ApplySavedSettings()
    {
        ApplySettings(GetSavedResolutionIndex(), GetSavedFullscreen(), GetSavedMusicVolume());
    }

    private void ApplySettings(int resolutionIndex, bool fullscreen, float musicVolume)
    {
        var size = GetWindowSize(resolutionIndex);
        Screen.SetResolution(size.x, size.y, fullscreen);
        ApplyMusicVolume(musicVolume);
    }

    private void ApplyMusicVolume(float musicVolume)
    {
        if (mainAudioMixer == null)
            return;

        mainAudioMixer.SetFloat(MusicVolumeMixerParameter, GetMusicVolumeDecibels(musicVolume));
    }

    public static float GetMusicVolumeDecibels(float musicVolume)
    {
        var scaledVolume = Mathf.Clamp01(musicVolume) * MaxMusicVolumeMultiplier;
        return scaledVolume <= 0.0001f ? -80f : Mathf.Log10(scaledVolume) * 20f;
    }

    private static Vector2Int GetWindowSize(int index)
    {
        return WindowSizes[Mathf.Clamp(index, 0, WindowSizes.Length - 1)];
    }
}
