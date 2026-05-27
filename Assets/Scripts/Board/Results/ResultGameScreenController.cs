using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/// <summary>
/// Отвечает за завершение партии и экран результата, связанные с ResultGameScreenController
/// </summary>

public sealed class ResultGameScreenController : MonoBehaviour
{
    [SerializeField] private Image resultImage;
    [SerializeField] private Sprite winSprite;
    [SerializeField] private Sprite loseSprite;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    /// <summary>
    /// Показывает картинку результата и подписывает кнопку возврата в главное меню
    /// </summary>
    private void OnEnable()
    {
        ApplyResultImage();

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(LoadMainMenu);
    }
    /// <summary>
    /// Отписывается от событий и останавливает временные процессы при выключении объекта
    /// </summary>
    private void OnDisable()
    {
        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(LoadMainMenu);
    }
    /// <summary>
    /// Применяет изменение к игровому или визуальному состоянию
    /// </summary>
    private void ApplyResultImage()
    {
        if (resultImage == null)
            return;

        var result = GameResultContext.HasResult ? GameResultContext.CurrentResult : GameResultType.Lose;
        resultImage.sprite = result == GameResultType.Win ? winSprite : loseSprite;
        resultImage.enabled = resultImage.sprite != null;
        resultImage.preserveAspect = true;
    }
    /// <summary>
    /// Загружает данные или сцену
    /// </summary>
    private void LoadMainMenu()
    {
        GameSaveService.DeleteSave();
        GameResultContext.Clear();
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
