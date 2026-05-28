using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Показывает игроку окно боя, кнопки боя, статусные подсказки и таблицу силы игрока и врага
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
    /// Показывает окно боя с монстром, силой сторон и доступными действиями
    /// </summary>
    /// <param name="data">Данные, которыми нужно заполнить окно боя</param>
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
    /// Меняет текст главной кнопки в окне боя
    /// </summary>
    /// <param name="buttonText">Новая подпись главной кнопки</param>
    public void SetActionButtonText(string buttonText)
    {
        SetText(actionButtonText, buttonText);
    }
    /// <summary>
    /// Показывает временную подсказку в окне боя
    /// </summary>
    /// <param name="message">Текст подсказки для игрока</param>
    /// <param name="duration">Время показа подсказки в секундах</param>
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
    /// Показывает постоянную подсказку в окне боя
    /// </summary>
    /// <param name="message">Текст подсказки для игрока</param>
    public void ShowPersistentStatus(string message)
    {
        StopClearStatusRoutine();
        SetText(statusText, message);
    }
    /// <summary>
    /// Очищает текст подсказки в окне боя
    /// </summary>
    public void ClearStatus()
    {
        StopClearStatusRoutine();
        SetText(statusText, string.Empty);
    }
    /// <summary>
    /// Закрывает окно боя и очищает его временные подсказки
    /// </summary>
    public void Hide()
    {
        ClearStatus();
        gameObject.SetActive(false);
    }
    /// <summary>
    /// Подписывает главную кнопку окна боя на нажатие
    /// </summary>
    private void OnEnable()
    {
        if (resolveButton != null)
            resolveButton.onClick.AddListener(HandleResolveClicked);
    }
    /// <summary>
    /// Отписывает кнопку боя и очищает временную подсказку
    /// </summary>
    private void OnDisable()
    {
        StopClearStatusRoutine();

        if (resolveButton != null)
            resolveButton.onClick.RemoveListener(HandleResolveClicked);
    }
    /// <summary>
    /// Сообщает системе боя, что игрок нажал главную кнопку окна боя
    /// </summary>
    private void HandleResolveClicked()
    {
        ResolveRequested?.Invoke();
    }
    /// <summary>
    /// Ждет указанное время и очищает временную подсказку боя
    /// </summary>
    /// <param name="duration">Время показа подсказки в секундах</param>
    private IEnumerator ClearStatusAfter(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        clearStatusRoutine = null;
        SetText(statusText, string.Empty);
    }
    /// <summary>
    /// Останавливает отложенное скрытие подсказки боя
    /// </summary>
    private void StopClearStatusRoutine()
    {
        if (clearStatusRoutine == null)
            return;

        StopCoroutine(clearStatusRoutine);
        clearStatusRoutine = null;
    }
    /// <summary>
    /// Подставляет строку в TMP-текст, если ссылка на него задана
    /// </summary>
    /// <param name="text">TMP-текст, в который нужно подставить значение</param>
    /// <param name="value">Строка, которую нужно показать в тексте</param>
    private static void SetText(TextMeshProUGUI text, string value)
    {
        if (text != null)
            text.text = value;
    }
    /// <summary>
    /// Подставляет спрайт в Image и скрывает картинку, если спрайта нет
    /// </summary>
    /// <param name="image">UI-картинка, в которую нужно подставить спрайт</param>
    /// <param name="sprite">Спрайт, который нужно показать</param>
    private static void SetImage(Image image, Sprite sprite)
    {
        if (image == null)
            return;

        image.sprite = sprite;
        image.enabled = sprite != null;
    }
    /// <summary>
    /// Заполняет список строк, из которых складывается сила игрока или монстра
    /// </summary>
    /// <param name="entries">Строки расчета силы</param>
    /// <param name="listRoot">Контейнер, в котором создаются строки силы</param>
    /// <param name="rowPrefab">Префаб одной строки силы</param>
    /// <param name="fallbackText">Запасной текстовый вывод, если префабные строки не настроены</param>
    /// <param name="childAlignment">Выравнивание строк внутри контейнера</param>
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
    /// Показывает итоговую силу в нижней строке расчета
    /// </summary>
    /// <param name="label">Подпись, которую увидит игрок</param>
    /// <param name="value">Итоговая сила, которую нужно показать</param>
    /// <param name="totalRow">Префабная строка итоговой силы</param>
    /// <param name="fallbackText">Запасной текстовый вывод, если префабные строки не настроены</param>
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
    /// Настраивает контейнер строк силы, чтобы новые строки выравнивались как нужно окну боя
    /// </summary>
    /// <param name="listRoot">Контейнер, в котором создаются строки силы</param>
    /// <param name="childAlignment">Выравнивание строк внутри контейнера</param>
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
    /// Собирает строки силы в один многострочный текст для старой версии окна боя
    /// </summary>
    /// <param name="entries">Строки расчета силы</param>
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
