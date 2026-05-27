using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Отвечает за визуальное отображение окна боя и не принимает игровых решений
/// </summary>

public sealed class BattleModalView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image playerPortraitImage;
    [SerializeField] private TextMeshProUGUI playerPowerText;
    [SerializeField] private Transform playerPowerListRoot;
    [SerializeField] private BattlePowerEntryRowView playerPowerEntryRowPrefab;
    [SerializeField] private TextMeshProUGUI playerTotalText;
    [SerializeField] private BattlePowerTotalRowView playerTotalRow;
    [SerializeField] private TextMeshProUGUI enemyNameText;
    [SerializeField] private Image enemyPortraitImage;
    [SerializeField] private TextMeshProUGUI enemyPowerText;
    [SerializeField] private Transform enemyPowerListRoot;
    [SerializeField] private BattlePowerEntryRowView enemyPowerEntryRowPrefab;
    [SerializeField] private TextMeshProUGUI enemyTotalText;
    [SerializeField] private BattlePowerTotalRowView enemyTotalRow;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    [SerializeField] private Button resolveButton;

    private Coroutine clearStatusRoutine;

    public event Action ResolveRequested;
    /// <summary>
    /// Показывает нужное окно или визуальное состояние игроку
    /// </summary>
    public void Show(BattleModalData data)
    {
        if (data == null)
            return;

        SetText(playerNameText, data.PlayerName);
        RenderPowerEntries(data.PlayerPowerEntries, playerPowerListRoot, playerPowerEntryRowPrefab, playerPowerText, TextAnchor.UpperLeft);
        RenderTotal("Итого", data.PlayerTotalPower, playerTotalRow, playerTotalText);

        SetText(enemyNameText, data.EnemyName);
        SetImage(enemyPortraitImage, data.EnemySprite);
        RenderPowerEntries(data.EnemyPowerEntries, enemyPowerListRoot, enemyPowerEntryRowPrefab, enemyPowerText, TextAnchor.UpperRight);
        RenderTotal("Итого", data.EnemyTotalPower, enemyTotalRow, enemyTotalText);

        gameObject.SetActive(true);
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода UpdateState
    /// </summary>
    public void UpdateState(string status, string buttonText)
    {
        ShowPersistentStatus(status);
        SetActionButtonText(buttonText);
    }
    /// <summary>
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
    /// </summary>
    public void SetActionButtonText(string buttonText)
    {
        SetText(actionButtonText, buttonText);
    }
    /// <summary>
    /// Показывает нужное окно или визуальное состояние игроку
    /// </summary>
    public void ShowTemporaryStatus(string message, float duration)
    {
        StopClearStatusRoutine();
        SetText(statusText, message);

        if (duration <= 0f)
        {
            ClearStatus();
            return;
        }

        clearStatusRoutine = StartCoroutine(ClearStatusAfter(duration));
    }
    /// <summary>
    /// Показывает нужное окно или визуальное состояние игроку
    /// </summary>
    public void ShowPersistentStatus(string message)
    {
        StopClearStatusRoutine();
        SetText(statusText, message);
    }
    /// <summary>
    /// Очищает текущее состояние и возвращает систему к пустому виду
    /// </summary>
    public void ClearStatus()
    {
        StopClearStatusRoutine();
        SetText(statusText, string.Empty);
    }
    /// <summary>
    /// Скрывает нужное окно или визуальное состояние
    /// </summary>
    public void Hide()
    {
        ClearStatus();
        gameObject.SetActive(false);
    }
    /// <summary>
    /// Подписывается на события и обновляет визуальное состояние при включении объекта
    /// </summary>
    private void OnEnable()
    {
        if (resolveButton != null)
            resolveButton.onClick.AddListener(HandleResolveClicked);
    }
    /// <summary>
    /// Отписывается от событий и останавливает временные процессы при выключении объекта
    /// </summary>
    private void OnDisable()
    {
        StopClearStatusRoutine();

        if (resolveButton != null)
            resolveButton.onClick.RemoveListener(HandleResolveClicked);
    }
    /// <summary>
    /// Обрабатывает событие от UI или другой игровой системы
    /// </summary>
    private void HandleResolveClicked()
    {
        ResolveRequested?.Invoke();
    }
    /// <summary>
    /// Очищает текущее состояние и возвращает систему к пустому виду
    /// </summary>
    private IEnumerator ClearStatusAfter(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        clearStatusRoutine = null;
        SetText(statusText, string.Empty);
    }
    /// <summary>
    /// Останавливает текущий процесс, корутину или визуальный переход
    /// </summary>
    private void StopClearStatusRoutine()
    {
        if (clearStatusRoutine == null)
            return;

        StopCoroutine(clearStatusRoutine);
        clearStatusRoutine = null;
    }
    /// <summary>
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
    /// </summary>
    private static void SetText(TextMeshProUGUI text, string value)
    {
        if (text != null)
            text.text = value;
    }
    /// <summary>
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
    /// </summary>
    private static void SetImage(Image image, Sprite sprite)
    {
        if (image == null)
            return;

        image.sprite = sprite;
        image.enabled = sprite != null;
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода RenderPowerEntries
    /// </summary>
    private static void RenderPowerEntries(
        IReadOnlyList<BattlePowerEntry> entries,
        Transform listRoot,
        BattlePowerEntryRowView rowPrefab,
        TextMeshProUGUI fallbackText,
        TextAnchor childAlignment)
    {
        if (listRoot == null || rowPrefab == null)
        {
            if (fallbackText != null)
            {
                fallbackText.enabled = true;
                fallbackText.text = FormatEntries(entries);
            }

            return;
        }

        ConfigurePowerListRoot(listRoot, childAlignment);

        if (fallbackText != null)
        {
            fallbackText.text = string.Empty;
            fallbackText.enabled = false;
        }

        for (var i = listRoot.childCount - 1; i >= 0; i--)
        {
            var child = listRoot.GetChild(i).gameObject;
            child.SetActive(false);
            UnityEngine.Object.Destroy(child);
        }

        if (entries == null)
            return;

        for (var i = 0; i < entries.Count; i++)
        {
            var row = Instantiate(rowPrefab, listRoot);
            row.gameObject.SetActive(true);
            row.Bind(entries[i]);
        }
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода RenderTotal
    /// </summary>
    private static void RenderTotal(string label, int value, BattlePowerTotalRowView totalRow, TextMeshProUGUI fallbackText)
    {
        if (totalRow != null)
        {
            totalRow.Bind(label, value);

            if (fallbackText != null)
            {
                fallbackText.text = string.Empty;
                fallbackText.enabled = false;
            }

            return;
        }

        if (fallbackText != null)
        {
            fallbackText.enabled = true;
            fallbackText.text = $"{label}: {value}";
        }
    }
    /// <summary>
    /// Настраивает ссылки и параметры, которые нужны компоненту для работы
    /// </summary>
    private static void ConfigurePowerListRoot(Transform listRoot, TextAnchor childAlignment)
    {
        var layoutGroup = listRoot.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
            layoutGroup = listRoot.gameObject.AddComponent<VerticalLayoutGroup>();

        layoutGroup.padding = new RectOffset(0, 0, 0, 0);
        layoutGroup.spacing = 8f;
        layoutGroup.childAlignment = childAlignment;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;
    }
    /// <summary>
    /// Формирует текст для отображения или вывода в лог
    /// </summary>
    private static string FormatEntries(IReadOnlyList<BattlePowerEntry> entries)
    {
        if (entries == null || entries.Count == 0)
            return string.Empty;

        var builder = new StringBuilder();
        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            builder.Append(entry.Label);
            builder.Append(": ");
            builder.Append(entry.Value);

            if (i < entries.Count - 1)
                builder.AppendLine();
        }

        return builder.ToString();
    }
}
