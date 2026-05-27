using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/// <summary>
/// Показывает картинку победы или поражения и возвращает игрока в главное меню
/// </summary>

public sealed class ResultGameScreenController : MonoBehaviour
{
    [SerializeField] private Image resultImage;
    [SerializeField] private Sprite winSprite;
    [SerializeField] private Sprite loseSprite;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    /// <summary>
    /// Включает подписки и обновляет отображение, когда объект становится активным
    /// </summary>
    private void OnEnable()
    {
        ApplyResultImage();

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(LoadMainMenu);
    }
    /// <summary>
    /// Отключает подписки и временные процессы, когда объект выключается
    /// </summary>
    private void OnDisable()
    {
        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(LoadMainMenu);
    }
    /// <summary>
    /// Подставляет картинку победы или поражения на финальном экране
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
    /// Возвращает игрока в главное меню
    /// </summary>
    private void LoadMainMenu()
    {
        GameSaveService.DeleteSave();
        GameResultContext.Clear();
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
