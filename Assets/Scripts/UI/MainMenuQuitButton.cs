using UnityEngine;
using UnityEngine.UI;

public sealed class MainMenuQuitButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private void OnEnable()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(QuitGame);
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(QuitGame);
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

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
