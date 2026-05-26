using UnityEngine;
using UnityEngine.UI;

public sealed class CloseButtonSpriteSkin : MonoBehaviour
{
    private const string DefaultSpriteSetPath = "UI/CloseButtonSprites";

    [SerializeField] private Button button;
    [SerializeField] private Image targetImage;
    [SerializeField] private CloseButtonSpriteSet spriteSet;

    private SmoothSpriteButton spriteTransition;

    private void Reset()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        Apply();
    }

    private void OnValidate()
    {
        ResolveReferences();
    }

    public static void ApplyTo(Button targetButton)
    {
        if (targetButton == null)
            return;

        var skin = targetButton.GetComponent<CloseButtonSpriteSkin>();
        if (skin == null)
            skin = targetButton.gameObject.AddComponent<CloseButtonSpriteSkin>();

        skin.button = targetButton;
        skin.Apply();
    }

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

    private void ResolveReferences()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (targetImage == null)
            targetImage = GetComponent<Image>();
    }

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

    private static bool IsCloseMarker(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var trimmed = value.Trim();
        return trimmed == "X" || trimmed == "x";
    }
}
