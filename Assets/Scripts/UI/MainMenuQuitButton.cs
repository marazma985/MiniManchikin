using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Кнопка Выход в главном меню, которая закрывает игру
/// </summary>

public sealed class MainMenuQuitButton : MonoBehaviour
{
    [SerializeField] private Button button;

    /// <summary>
    /// Подписывает кнопку выхода из игры на нажатие
    /// </summary>
    private void OnEnable()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(QuitGame);
    }
    /// <summary>
    /// Отписывает кнопку выхода из игры от нажатия
    /// </summary>
    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(QuitGame);
    }
    /// <summary>
    /// Автоматически находит Button для выхода из игры
    /// </summary>
    private void Reset()
    {
        button = GetComponent<Button>();
    }
    /// <summary>
    /// Находит кнопку выхода в инспекторе, если ссылка еще не задана
    /// </summary>
    private void OnValidate()
    {
        if (button == null)
            button = GetComponent<Button>();
    }
    /// <summary>
    /// Закрывает игру в сборке или останавливает Play Mode в редакторе
    /// </summary>
    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
