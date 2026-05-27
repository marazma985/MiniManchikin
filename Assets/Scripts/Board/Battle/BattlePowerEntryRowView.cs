using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Визуальная строка в окне боя, которая показывает один источник силы и его значение
/// </summary>

public sealed class BattlePowerEntryRowView : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private float fontSize = 22f;

    /// <summary>
    /// Заполняет визуальный элемент свежими данными перед показом игроку
    /// </summary>
    /// <param name="entry">Строка расчета силы, которую нужно показать</param>
    public void Bind(BattlePowerEntry entry)
    {
        EnsureReferences();

        if (labelText != null)
            labelText.text = entry.Label;

        if (valueText != null)
            valueText.text = entry.Value.ToString();
    }
    /// <summary>
    /// Готовит строку с названием бонуса и его числом в окне боя
    /// </summary>
    private void Awake()
    {
        EnsureReferences();
    }
    /// <summary>
    /// Перестраивает строку силы в инспекторе после изменения настроек
    /// </summary>
    private void OnValidate()
    {
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
    }
    /// <summary>
    /// Создает подпись и значение строки силы, если они еще не назначены
    /// </summary>
    private void EnsureReferences()
    {
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (backgroundImage != null)
        {
            backgroundImage.color = Color.white;
            backgroundImage.raycastTarget = false;
        }

        if (labelText == null)
            labelText = CreateText("Label", TextAlignmentOptions.MidlineLeft);

        if (valueText == null)
            valueText = CreateText("Value", TextAlignmentOptions.MidlineRight);

        ConfigureText(labelText, TextAlignmentOptions.MidlineLeft);
        ConfigureText(valueText, TextAlignmentOptions.MidlineRight);
    }
    /// <summary>
    /// Создает текстовую ячейку для строки расчета силы
    /// </summary>
    /// <param name="textObjectName">Имя создаваемого текстового объекта</param>
    /// <param name="alignment">Выравнивание текста внутри строки</param>
    private TextMeshProUGUI CreateText(string textObjectName, TextAlignmentOptions alignment)
    {
        var textObject = new GameObject(textObjectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.layer = gameObject.layer;

        var textTransform = textObject.GetComponent<RectTransform>();
        textTransform.SetParent(transform, false);

        var text = textObject.GetComponent<TextMeshProUGUI>();
        ConfigureText(text, alignment);
        return text;
    }
    /// <summary>
    /// Настраивает текстовую ячейку строки силы: шрифт, цвет и выравнивание
    /// </summary>
    /// <param name="text">TMP-текст, в который нужно подставить значение</param>
    /// <param name="alignment">Выравнивание текста внутри строки</param>
    private void ConfigureText(TextMeshProUGUI text, TextAlignmentOptions alignment)
    {
        if (text == null)
            return;

        text.color = textColor;
        text.fontSize = fontSize;
        text.fontSizeMin = 16f;
        text.fontSizeMax = fontSize;
        text.enableAutoSizing = true;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
        text.alignment = alignment;

        var rectTransform = text.rectTransform;
        rectTransform.anchorMin = alignment == TextAlignmentOptions.MidlineRight ? new Vector2(1f, 0f) : Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = alignment == TextAlignmentOptions.MidlineRight ? new Vector2(1f, 0.5f) : new Vector2(0f, 0.5f);

        if (alignment == TextAlignmentOptions.MidlineRight)
        {
            rectTransform.offsetMin = new Vector2(-66f, 4f);
            rectTransform.offsetMax = new Vector2(-18f, -4f);
        }
        else
        {
            rectTransform.offsetMin = new Vector2(18f, 4f);
            rectTransform.offsetMax = new Vector2(-70f, -4f);
        }
    }
}
