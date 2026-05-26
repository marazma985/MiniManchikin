using UnityEngine;

public static class BackgroundMusicBootstrap
{
    private const string PlayerPrefabPath = "Audio/BackgroundMusicPlayer";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureMusicPlayer()
    {
        if (BackgroundMusicPlayer.Instance != null || Object.FindAnyObjectByType<BackgroundMusicPlayer>() != null)
            return;

        var prefab = Resources.Load<GameObject>(PlayerPrefabPath);
        if (prefab == null)
        {
            Debug.LogWarning($"Background music prefab is missing at Resources/{PlayerPrefabPath}.");
            return;
        }

        var instance = Object.Instantiate(prefab);
        if (instance.GetComponent<BackgroundMusicPlayer>() == null)
        {
            Debug.LogWarning($"Background music prefab at Resources/{PlayerPrefabPath} has no {nameof(BackgroundMusicPlayer)} component.");
            Object.Destroy(instance);
        }
    }
}
