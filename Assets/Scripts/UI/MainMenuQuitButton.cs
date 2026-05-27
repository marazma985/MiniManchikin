using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Отвечает за часть игровой логики или интерфейса, связанную с MainMenuQuitButton
/// </summary>

public sealed class MainMenuQuitButton : MonoBehaviour
{
    [SerializeField] private Button button;

    /// <summary>
    /// Подписывается на кнопку выхода при включении объекта
    /// </summary>
    private void OnEnable()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(QuitGame);
    }
    /// <summary>
    /// Отписывается от событий и останавливает временные процессы при выключении объекта
    /// </summary>
    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(QuitGame);
    }
    /// <summary>
    /// Заполняет стандартные ссылки при добавлении компонента в редакторе Unity
    /// </summary>
    private void Reset()
    {
        button = GetComponent<Button>();
    }
    /// <summary>
    /// Поддерживает корректные значения и ссылки при изменениях в инспекторе Unity
    /// </summary>
    private void OnValidate()
    {
        if (button == null)
            button = GetComponent<Button>();
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода QuitGame
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
