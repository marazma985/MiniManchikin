using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BattlePowerEntryRowView : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private float fontSize = 22f;

    public void Bind(BattlePowerEntry entry)
    {
        EnsureReferences();

        if (labelText != null)
            labelText.text = entry.Label;

        if (valueText != null)
            valueText.text = entry.Value.ToString();
    }

    private void Awake()
    {
        EnsureReferences();
    }

    private void OnValidate()
    {
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
    }

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
