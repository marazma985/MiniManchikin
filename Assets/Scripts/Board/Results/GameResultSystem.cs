using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// Отвечает за завершение партии и экран результата, связанные с GameResultSystem
/// </summary>

public sealed class GameResultSystem : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    [SerializeField, Min(1)] private int winningLevel = 10;
    [SerializeField] private string resultSceneName = "ResultGameScene";

    private bool resultTriggered;
    /// <summary>
    /// Подписывается на события и обновляет визуальное состояние при включении объекта
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
    /// Выполняет настройку после того, как Unity инициализировал объекты сцены
    /// </summary>
    private void Start()
    {
        EvaluateResult();
    }
    /// <summary>
    /// Отписывается от событий и останавливает временные процессы при выключении объекта
    /// </summary>
    private void OnDisable()
    {
        if (playerStats == null)
            return;

        playerStats.OnHpChanged -= HandleHpChanged;
        playerStats.OnLevelChanged -= HandleLevelChanged;
    }
    /// <summary>
    /// Поддерживает корректные значения и ссылки при изменениях в инспекторе Unity
    /// </summary>
    private void OnValidate()
    {
        winningLevel = Mathf.Max(1, winningLevel);
    }
    /// <summary>
    /// Обрабатывает событие от UI или другой игровой системы
    /// </summary>
    private void HandleHpChanged(int currentHp, int maxHp)
    {
        EvaluateResult();
    }
    /// <summary>
    /// Обрабатывает событие от UI или другой игровой системы
    /// </summary>
    private void HandleLevelChanged(int level)
    {
        EvaluateResult();
    }
    /// <summary>
    /// Проверяет условия и выбирает следующее состояние игры
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
    /// Открывает игровой экран, модальное окно или состояние результата
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
