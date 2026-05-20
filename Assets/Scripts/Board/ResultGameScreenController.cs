using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class ResultGameScreenController : MonoBehaviour
{
    [SerializeField] private Image resultImage;
    [SerializeField] private Sprite winSprite;
    [SerializeField] private Sprite loseSprite;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void OnEnable()
    {
        ApplyResultImage();

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(LoadMainMenu);
    }

    private void OnDisable()
    {
        if (mainMenuButton != null)
            mainMenuButton.onClick.RemoveListener(LoadMainMenu);
    }

    private void ApplyResultImage()
    {
        if (resultImage == null)
            return;

        var result = GameResultContext.HasResult ? GameResultContext.CurrentResult : GameResultType.Lose;
        resultImage.sprite = result == GameResultType.Win ? winSprite : loseSprite;
        resultImage.enabled = resultImage.sprite != null;
        resultImage.preserveAspect = true;
    }

    private void LoadMainMenu()
    {
        GameResultContext.Clear();
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
