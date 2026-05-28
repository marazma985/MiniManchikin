using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Автоматически заменяет обычный крестик окна на общий красивый крестик с плавной сменой картинки
/// </summary>

public sealed class CloseButtonSpriteSkin : MonoBehaviour
{
    private const string DefaultSpriteSetPath = "UI/CloseButtonSprites";

    [SerializeField] private Button button;
    [SerializeField] private Image targetImage;
    [SerializeField] private CloseButtonSpriteSet spriteSet;

    private SmoothSpriteButton spriteTransition;
    /// <summary>
    /// Автоматически находит Button для нового крестика закрытия
    /// </summary>
    private void Reset()
    {
        ResolveReferences();
    }
    /// <summary>
    /// Применяет спрайты крестика при включении кнопки закрытия
    /// </summary>
    private void OnEnable()
    {
        Apply();
    }
    /// <summary>
    /// Применяет спрайты крестика в инспекторе после изменения настроек
    /// </summary>
    private void OnValidate()
    {
        ResolveReferences();
    }
    /// <summary>
    /// Настраивает кнопку закрытия на обычный и hover-спрайт крестика
    /// </summary>
    public void Apply()
    {
        ResolveReferences();

        if (button == null || targetImage == null)
            return;

        if (spriteSet == null)
            spriteSet = Resources.Load<CloseButtonSpriteSet>(DefaultSpriteSetPath);

        if (spriteSet == null || spriteSet.NormalSprite == null)
            return;

        targetImage.sprite = spriteSet.NormalSprite;
        targetImage.color = Color.white;
        targetImage.enabled = true;
        targetImage.preserveAspect = true;
        button.targetGraphic = targetImage;
        button.transition = Selectable.Transition.None;

        HideTextMarkers();

        if (spriteTransition == null)
            spriteTransition = button.GetComponent<SmoothSpriteButton>();

        if (spriteTransition == null)
            spriteTransition = button.gameObject.AddComponent<SmoothSpriteButton>();

        spriteTransition.Configure(button, targetImage, spriteSet.NormalSprite, spriteSet.HighlightedSprite, spriteSet.PressedSprite, null, spriteSet.FadeDuration);
    }
    /// <summary>
    /// Доводит текущую игровую ситуацию до следующего шага
    /// </summary>
    private void ResolveReferences()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (targetImage == null)
            targetImage = GetComponent<Image>();
    }
    /// <summary>
    /// Прячет старые текстовые крестики внутри кнопки
    /// </summary>
    private void HideTextMarkers()
    {
        var textComponents = GetComponentsInChildren<Text>(true);
        for (var i = 0; i < textComponents.Length; i++)
        {
            var text = textComponents[i];
            if (text != null && IsCloseMarker(text.text))
                text.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// Проверяет, похож ли текст на старый символ закрытия
    /// </summary>
    private static bool IsCloseMarker(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var trimmed = value.Trim();
        return trimmed == "X" || trimmed == "x";
    }
}
