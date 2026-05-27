using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Кнопка главного меню со спрайтами для разных состояний: обычная, наведенная и заблокированная
/// </summary>

public sealed class MainMenuSpriteButton : MonoBehaviour
{
    /// <summary>
    /// Набор вариантов, из которых игра выбирает нужное состояние для VisualKind
    /// </summary>
    public enum VisualKind
    {
        Continue,
        NewGame,
        Settings,
        Exit
    }

    [SerializeField] private VisualKind kind;
    [SerializeField] private bool continueAvailable;
    [SerializeField] private Image targetImage;
    [SerializeField] private Button button;

    [Header("Sprites")]
    [SerializeField] private Sprite continueAvailableSprite;
    [SerializeField] private Sprite continueLockedSprite;
    [SerializeField] private Sprite newGameSprite;
    [SerializeField] private Sprite settingsSprite;
    [SerializeField] private Sprite exitSprite;

    public VisualKind Kind
    {
        get => kind;
        set
        {
            kind = value;
            ApplyVisual();
        }
    }

    public bool ContinueAvailable
    {
        get => continueAvailable;
        set
        {
            continueAvailable = value;
            ApplyVisual();
        }
    }
    /// <summary>
    /// Заполняет удобные значения по умолчанию при добавлении компонента в Unity
    /// </summary>
    private void Reset()
    {
        targetImage = GetComponent<Image>();
        button = GetComponent<Button>();
    }
    /// <summary>
    /// Находит компоненты кнопки главного меню и выставляет стартовый спрайт
    /// </summary>
    private void Awake()
    {
        ApplyVisual();
    }
    /// <summary>
    /// Помогает держать настройки компонента корректными прямо в инспекторе Unity
    /// </summary>
    private void OnValidate()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();
        if (button == null)
            button = GetComponent<Button>();

        ApplyVisual();
    }
    [ContextMenu("Apply Visual")]
    /// <summary>
    /// Обновляет картинку кнопки главного меню под ее текущий режим
    /// </summary>
    public void ApplyVisual()
    {
        if (targetImage == null)
            return;

        targetImage.sprite = PickSprite();
        targetImage.color = Color.white;
        targetImage.preserveAspect = true;
        targetImage.type = Image.Type.Simple;

        if (button != null)
        {
            button.transition = Selectable.Transition.None;
            button.interactable = kind != VisualKind.Continue || continueAvailable;
        }
    }
    /// <summary>
    /// Выбирает подходящий вариант
    /// </summary>
    private Sprite PickSprite()
    {
        switch (kind)
        {
            case VisualKind.Continue:
                return continueAvailable ? continueAvailableSprite : continueLockedSprite;
            case VisualKind.NewGame:
                return newGameSprite;
            case VisualKind.Settings:
                return settingsSprite;
            case VisualKind.Exit:
                return exitSprite;
            default:
                return null;
        }
    }
}
