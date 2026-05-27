using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MainMenuSceneLoader : MonoBehaviour
{
    private enum LaunchMode
    {
        NewGame,
        Continue
    }

    [SerializeField] private Button button;
    [SerializeField] private string sceneName = "BoardGame";
    [SerializeField] private LaunchMode launchMode = LaunchMode.NewGame;
    [SerializeField] private MainMenuSpriteButton spriteButton;

    private void OnEnable()
    {
        RefreshContinueState();

        if (button != null)
            button.onClick.AddListener(LoadScene);
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(LoadScene);
    }

    private void Reset()
    {
        button = GetComponent<Button>();
        spriteButton = GetComponent<MainMenuSpriteButton>();
    }

    private void OnValidate()
    {
        if (button == null)
            button = GetComponent<Button>();
        if (spriteButton == null)
            spriteButton = GetComponent<MainMenuSpriteButton>();

        RefreshContinueState();
    }

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
