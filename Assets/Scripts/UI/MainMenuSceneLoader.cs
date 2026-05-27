using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/// <summary>
/// Кнопки главного меню, которые запускают новую игру или продолжают сохранение
/// </summary>

public sealed class MainMenuSceneLoader : MonoBehaviour
{
    /// <summary>
/// Режим запуска игровой сцены: новая партия или продолжение сохранения
    /// </summary>
    private enum LaunchMode
    {
        NewGame,
        Continue
    }

    [SerializeField] private Button button;
    [SerializeField] private string sceneName = "BoardGame";
    [SerializeField] private LaunchMode launchMode = LaunchMode.NewGame;
    [SerializeField] private MainMenuSpriteButton spriteButton;

    /// <summary>
    /// Подписывает кнопку меню на запуск новой игры или продолжение
    /// </summary>
    private void OnEnable()
    {
        RefreshContinueState();

        if (button != null)
            button.onClick.AddListener(LoadScene);
    }
    /// <summary>
    /// Отписывает кнопку меню от запуска игры
    /// </summary>
    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(LoadScene);
    }
    /// <summary>
    /// Автоматически находит Button для пункта главного меню
    /// </summary>
    private void Reset()
    {
        button = GetComponent<Button>();
        spriteButton = GetComponent<MainMenuSpriteButton>();
    }
    /// <summary>
    /// Находит кнопку главного меню в инспекторе, если ссылка еще не задана
    /// </summary>
    private void OnValidate()
    {
        if (button == null)
            button = GetComponent<Button>();
        if (spriteButton == null)
            spriteButton = GetComponent<MainMenuSpriteButton>();

        RefreshContinueState();
    }
    /// <summary>
    /// Загружает сцену, файл или данные
    /// </summary>
    private void LoadScene()
    {
        if (launchMode == LaunchMode.Continue)
        {
            if (!GameSaveService.HasSave())
                return;

            GameLaunchIntent.Set(GameLaunchMode.Continue);
        }
        else
        {
            GameSaveService.DeleteSave();
            GameLaunchIntent.Set(GameLaunchMode.NewGame);
        }

        SceneManager.LoadScene(sceneName);
    }
    /// <summary>
    /// Включает или блокирует кнопку продолжения в зависимости от наличия сохранения
    /// </summary>
    private void RefreshContinueState()
    {
        if (launchMode != LaunchMode.Continue)
            return;

        var hasSave = GameSaveService.HasSave();
        if (spriteButton != null)
            spriteButton.ContinueAvailable = hasSave;
        if (button != null)
            button.interactable = hasSave;
    }
}
