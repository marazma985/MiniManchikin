using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/// <summary>
/// Отвечает за часть игровой логики или интерфейса, связанную с MainMenuSceneLoader
/// </summary>

public sealed class MainMenuSceneLoader : MonoBehaviour
{
    /// <summary>
    /// Перечисляет варианты launch mode, которые используются в игровой логике вместо строковых значений
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
    /// Обновляет доступность продолжения и подписывается на кнопку загрузки сцены
    /// </summary>
    private void OnEnable()
    {
        RefreshContinueState();

        if (button != null)
            button.onClick.AddListener(LoadScene);
    }
    /// <summary>
    /// Отписывается от событий и останавливает временные процессы при выключении объекта
    /// </summary>
    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(LoadScene);
    }
    /// <summary>
    /// Заполняет стандартные ссылки при добавлении компонента в редакторе Unity
    /// </summary>
    private void Reset()
    {
        button = GetComponent<Button>();
        spriteButton = GetComponent<MainMenuSpriteButton>();
    }
    /// <summary>
    /// Поддерживает корректные значения и ссылки при изменениях в инспекторе Unity
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
    /// Загружает данные или сцену
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
    /// Обновляет отображение на основе текущих данных
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
