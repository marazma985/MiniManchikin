using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MainMenuSceneLoader : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private string sceneName = "BoardGame";

    private void OnEnable()
    {
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
    }

    private void OnValidate()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

    private void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
