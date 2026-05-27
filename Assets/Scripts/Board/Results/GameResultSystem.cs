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
    /// Подписывает проверку конца игры на здоровье и уровень игрока
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
    /// Отписывает проверку конца игры от здоровья и уровня игрока
    /// </summary>
    private void OnDisable()
    {
        if (playerStats == null)
            return;

        playerStats.OnHpChanged -= HandleHpChanged;
        playerStats.OnLevelChanged -= HandleLevelChanged;
    }
    /// <summary>
    /// Подключает PlayerStats в редакторе, если ссылка еще не задана
    /// </summary>
    private void OnValidate()
    {
        winningLevel = Mathf.Max(1, winningLevel);
    }
    /// <summary>
    /// Проверяет поражение после изменения здоровья игрока
    /// </summary>
    private void HandleHpChanged(int currentHp, int maxHp)
    {
        EvaluateResult();
    }
    /// <summary>
    /// Проверяет победу после изменения уровня игрока
    /// </summary>
    private void HandleLevelChanged(int level)
    {
        EvaluateResult();
    }
    /// <summary>
    /// Определяет, закончилась ли партия победой или поражением
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
