using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// Добавляет кнопкам главного меню легкое движение при наведении мыши
/// </summary>

public sealed class MainMenuButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform target;
    [SerializeField] private float hoverLift = 5f;
    [SerializeField] private float moveSmoothTime = 0.055f;

    private Vector2 baseAnchoredPosition;
    private Vector2 moveVelocity;
    private bool initialized;
    private bool hovering;
    /// <summary>
    /// Подключает звуковой или визуальный отклик кнопки главного меню
    /// </summary>
    private void Awake()
    {
        Initialize();
    }
    /// <summary>
    /// Включает подписки и обновляет отображение, когда объект становится активным
    /// </summary>
    private void OnEnable()
    {
        Initialize();
    }
    /// <summary>
    /// Помогает держать настройки компонента корректными прямо в инспекторе Unity
    /// </summary>
    private void OnValidate()
    {
        if (target == null)
            target = transform as RectTransform;
    }
    /// <summary>
    /// Каждый кадр проверяет ввод игрока или обновляет отображение
    /// </summary>
    private void Update()
    {
        Initialize();

        var desiredPosition = baseAnchoredPosition + (hovering ? Vector2.up * hoverLift : Vector2.zero);
        target.anchoredPosition = Vector2.SmoothDamp(target.anchoredPosition, desiredPosition, ref moveVelocity, moveSmoothTime);
    }
    /// <summary>
    /// Реагирует на мышь игрока и меняет вид элемента при наведении или нажатии
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }
    /// <summary>
    /// Реагирует на мышь игрока и меняет вид элемента при наведении или нажатии
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }
    /// <summary>
    /// Подготавливает объект к работе
    /// </summary>
    private void Initialize()
    {
        if (initialized)
            return;

        if (target == null)
            target = transform as RectTransform;

        if (target == null)
            return;

        baseAnchoredPosition = target.anchoredPosition;
        initialized = true;
    }
}
