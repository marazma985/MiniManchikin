using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Кнопка Выход в главном меню, которая закрывает игру
/// </summary>

public sealed class MainMenuQuitButton : MonoBehaviour
{
    [SerializeField] private Button button;

    /// <summary>
    /// Включает подписки и обновляет отображение, когда объект становится активным
    /// </summary>
    private void OnEnable()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(QuitGame);
    }
    /// <summary>
    /// Отключает подписки и временные процессы, когда объект выключается
    /// </summary>
    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(QuitGame);
    }
    /// <summary>
    /// Заполняет удобные значения по умолчанию при добавлении компонента в Unity
    /// </summary>
    private void Reset()
    {
        button = GetComponent<Button>();
    }
    /// <summary>
    /// Помогает держать настройки компонента корректными прямо в инспекторе Unity
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
