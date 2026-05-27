using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Отвечает за часть системы боя, связанную с BattlePowerTotalRowView
/// </summary>

public sealed class BattlePowerTotalRowView : MonoBehaviour
{
    [SerializeField] private Image dividerImage;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private float fontSize = 30f;
    [SerializeField] private float dividerHeight = 6f;

    /// <summary>
    /// Заполняет итоговую строку силы подписью и суммарным значением
    /// </summary>
    public void Bind(string label, int value)
    {
        EnsureReferences();

        if (labelText != null)
            labelText.text = label;

        if (valueText != null)
            valueText.text = value.ToString();
    }
    /// <summary>
    /// Инициализирует ссылки и внутреннее состояние до запуска сцены
    /// </summary>
    private void Awake()
    {
        EnsureReferences();
    }
    /// <summary>
    /// Поддерживает корректные значения и ссылки при изменениях в инспекторе Unity
    /// </summary>
    private void OnValidate()
    {
        if (dividerImage != null)
        {
            dividerImage.color = textColor;
            ConfigureDivider();
        }

        ConfigureText(labelText, TextAlignmentOptions.MidlineLeft);
        ConfigureText(valueText, TextAlignmentOptions.MidlineRight);
    }
    /// <summary>
    /// Гарантирует, что нужный объект, ресурс или ссылка существует
    /// </summary>
    private void EnsureReferences()
    {
        if (dividerImage == null)
            dividerImage = CreateDivider();

        if (dividerImage != null)
        {
            dividerImage.color = textColor;
            dividerImage.raycastTarget = false;
        }

        if (labelText == null)
            labelText = CreateText("Label", TextAlignmentOptions.MidlineLeft);

        if (valueText == null)
            valueText = CreateText("Value", TextAlignmentOptions.MidlineRight);

        ConfigureDivider();
        ConfigureText(labelText, TextAlignmentOptions.MidlineLeft);
        ConfigureText(valueText, TextAlignmentOptions.MidlineRight);
    }
    /// <summary>
    /// Создает объект или набор данных, который дальше использует система
    /// </summary>
    private Image CreateDivider()
    {
        var dividerObject = new GameObject("Divider", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        dividerObject.layer = gameObject.layer;
        dividerObject.transform.SetParent(transform, false);
        return dividerObject.GetComponent<Image>();
    }
    /// <summary>
    /// Создает объект или набор данных, который дальше использует система
    /// </summary>
    private TextMeshProUGUI CreateText(string textObjectName, TextAlignmentOptions alignment)
    {
        var textObject = new GameObject(textObjectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.layer = gameObject.layer;
        textObject.transform.SetParent(transform, false);

        var text = textObject.GetComponent<TextMeshProUGUI>();
        ConfigureText(text, alignment);
        return text;
    }
    /// <summary>
    /// Настраивает ссылки и параметры, которые нужны компоненту для работы
    /// </summary>
    private void ConfigureDivider()
    {
        if (dividerImage == null)
            return;

        var rectTransform = dividerImage.rectTransform;
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.offsetMin = new Vector2(0f, -dividerHeight);
        rectTransform.offsetMax = Vector2.zero;
    }
    /// <summary>
    /// Настраивает ссылки и параметры, которые нужны компоненту для работы
    /// </summary>
    private void ConfigureText(TextMeshProUGUI text, TextAlignmentOptions alignment)
    {
        if (text == null)
            return;

        text.color = textColor;
        text.fontSize = fontSize;
        text.fontSizeMin = 18f;
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
            rectTransform.offsetMin = new Vector2(-84f, 0f);
            rectTransform.offsetMax = new Vector2(0f, -dividerHeight - 2f);
        }
        else
        {
            rectTransform.offsetMin = new Vector2(0f, 0f);
            rectTransform.offsetMax = new Vector2(-92f, -dividerHeight - 2f);
        }
    }
}
