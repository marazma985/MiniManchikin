using UnityEngine;
using UnityEngine.UI;

public sealed class MainMenuSkinSlots : MonoBehaviour
{
    [Header("Scene Art")]
    [SerializeField] private Image background;
    [SerializeField] private Image logo;
    [SerializeField] private Image leftCharacter;
    [SerializeField] private Image rightCharacter;

    [Header("Buttons")]
    [SerializeField] private Image continueButton;
    [SerializeField] private Image newGameButton;
    [SerializeField] private Image settingsButton;
    [SerializeField] private Image exitButton;

    [Header("Sprites")]
    [SerializeField] private Sprite backgroundSprite;
    [SerializeField] private Sprite logoSprite;
    [SerializeField] private Sprite leftCharacterSprite;
    [SerializeField] private Sprite rightCharacterSprite;
    [SerializeField] private Sprite continueButtonSprite;
    [SerializeField] private Sprite newGameButtonSprite;
    [SerializeField] private Sprite settingsButtonSprite;
    [SerializeField] private Sprite exitButtonSprite;

    private void Awake()
    {
        ApplySkin();
    }

    private void OnValidate()
    {
        ApplySkin();
    }

    [ContextMenu("Apply Skin")]
    public void ApplySkin()
    {
        SetSprite(background, backgroundSprite, true);
        SetSprite(logo, logoSprite, true);
        SetSprite(leftCharacter, leftCharacterSprite, true);
        SetSprite(rightCharacter, rightCharacterSprite, true);
        SetSprite(continueButton, continueButtonSprite, false);
        SetSprite(newGameButton, newGameButtonSprite, false);
        SetSprite(settingsButton, settingsButtonSprite, false);
        SetSprite(exitButton, exitButtonSprite, false);
    }

    private static void SetSprite(Image image, Sprite sprite, bool preserveAspect)
    {
        if (image == null || sprite == null)
            return;

        image.sprite = sprite;
        image.preserveAspect = preserveAspect;
        image.type = Image.Type.Simple;
    }
}
