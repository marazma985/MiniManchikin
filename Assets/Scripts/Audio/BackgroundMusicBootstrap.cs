using UnityEngine;
/// <summary>
/// Следит, чтобы объект фоновой музыки был создан до загрузки первой сцены и не дублировался между сценами
/// </summary>

public static class BackgroundMusicBootstrap
{
    private const string PlayerPrefabPath = "Audio/BackgroundMusicPlayer";
    /// <summary>
    /// Проверяет наличие постоянного проигрывателя музыки и создает его из Resources при первом запуске игры
    /// </summary>
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
