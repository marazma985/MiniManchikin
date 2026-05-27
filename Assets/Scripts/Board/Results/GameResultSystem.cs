using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// Следит, победил или проиграл игрок, и открывает экран результата
/// </summary>

public sealed class GameResultSystem : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField, Min(1)] private int winningLevel = 10;
    [SerializeField] private string resultSceneName = "ResultGameScene";

    private bool resultTriggered;
    /// <summary>
    /// Включает подписки и обновляет отображение, когда объект становится активным
    /// </summary>
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
    /// <summary>
    /// Запускает начальную настройку после загрузки сцены
    /// </summary>
    private void Start()
    {
        EvaluateResult();
    }
    /// <summary>
    /// Отключает подписки и временные процессы, когда объект выключается
    /// </summary>
    private void OnDisable()
    {
        if (playerStats == null)
            return;

        playerStats.OnHpChanged -= HandleHpChanged;
        playerStats.OnLevelChanged -= HandleLevelChanged;
    }
    /// <summary>
    /// Помогает держать настройки компонента корректными прямо в инспекторе Unity
    /// </summary>
    private void OnValidate()
    {
        winningLevel = Mathf.Max(1, winningLevel);
    }
    /// <summary>
    /// Обрабатывает действие игрока или событие другой системы
    /// </summary>
    private void HandleHpChanged(int currentHp, int maxHp)
    {
        EvaluateResult();
    }
    /// <summary>
    /// Обрабатывает действие игрока или событие другой системы
    /// </summary>
    private void HandleLevelChanged(int level)
    {
        EvaluateResult();
    }
    /// <summary>
    /// Проверяет условия и выбирает дальнейший результат
    /// </summary>
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
    /// <summary>
    /// Открывает окно или экран для игрока
    /// </summary>
    private void OpenResult(GameResultType result)
    {
        if (resultTriggered)
            return;

        resultTriggered = true;
        if (GameSaveController.Instance != null)
            GameSaveController.Instance.DeleteSaveAndDisableSaving();
        else
            GameSaveService.DeleteSave();

        GameResultContext.SetResult(result);
        Debug.Log($"Game result triggered: {result}. Loading scene '{resultSceneName}'.");
        SceneManager.LoadScene(resultSceneName);
    }
}
