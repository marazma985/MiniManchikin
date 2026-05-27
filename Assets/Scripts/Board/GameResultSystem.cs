using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameResultSystem : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField, Min(1)] private int winningLevel = 10;
    [SerializeField] private string resultSceneName = "ResultGameScene";

    private bool resultTriggered;

    private void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.OnHpChanged += HandleHpChanged;
            playerStats.OnLevelChanged += HandleLevelChanged;
        }
        else
        {
            Debug.LogWarning("GameResultSystem requires PlayerStats reference.");
        }

        EvaluateResult();
    }

    private void Start()
    {
        EvaluateResult();
    }

    private void OnDisable()
    {
        if (playerStats == null)
            return;

        playerStats.OnHpChanged -= HandleHpChanged;
        playerStats.OnLevelChanged -= HandleLevelChanged;
    }

    private void OnValidate()
    {
        winningLevel = Mathf.Max(1, winningLevel);
    }

    private void HandleHpChanged(int currentHp, int maxHp)
    {
        EvaluateResult();
    }

    private void HandleLevelChanged(int level)
    {
        EvaluateResult();
    }

    private void EvaluateResult()
    {
        if (resultTriggered || playerStats == null)
            return;

        if (playerStats.Level >= winningLevel)
        {
            OpenResult(GameResultType.Win);
            return;
        }

        if (playerStats.CurrentHp <= 0)
            OpenResult(GameResultType.Lose);
    }

    private void OpenResult(GameResultType result)
    {
        if (resultTriggered)
            return;

        resultTriggered = true;
        GameSaveService.DeleteSave();
        GameResultContext.SetResult(result);
        Debug.Log($"Game result triggered: {result}. Loading scene '{resultSceneName}'.");
        SceneManager.LoadScene(resultSceneName);
    }

    [ContextMenu("Test Trigger Win Result")]
    private void TestTriggerWinResult()
    {
        OpenResult(GameResultType.Win);
    }

    [ContextMenu("Test Trigger Lose Result")]
    private void TestTriggerLoseResult()
    {
        OpenResult(GameResultType.Lose);
    }
}
