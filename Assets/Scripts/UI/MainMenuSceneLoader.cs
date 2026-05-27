using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/// <summary>
/// Кнопки главного меню, которые запускают новую игру или продолжают сохранение
/// </summary>

public sealed class MainMenuSceneLoader : MonoBehaviour
{
    /// <summary>
    /// Набор вариантов, из которых игра выбирает нужное состояние для LaunchMode
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
    /// Включает подписки и обновляет отображение, когда объект становится активным
    /// </summary>
    private void OnEnable()
    {
        RefreshContinueState();

        if (button != null)
            button.onClick.AddListener(LoadScene);
    }
    /// <summary>
    /// Отключает подписки и временные процессы, когда объект выключается
    /// </summary>
    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(LoadScene);
    }
    /// <summary>
    /// Заполняет удобные значения по умолчанию при добавлении компонента в Unity
    /// </summary>
    private void Reset()
    {
        button = GetComponent<Button>();
        spriteButton = GetComponent<MainMenuSpriteButton>();
    }
    /// <summary>
    /// Помогает держать настройки компонента корректными прямо в инспекторе Unity
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
