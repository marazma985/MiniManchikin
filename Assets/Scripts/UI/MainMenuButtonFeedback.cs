using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// Отвечает за часть игровой логики или интерфейса, связанную с MainMenuButtonFeedback
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
    /// Инициализирует ссылки и внутреннее состояние до запуска сцены
    /// </summary>
    private void Awake()
    {
        Initialize();
    }
    /// <summary>
    /// Подписывается на события и обновляет визуальное состояние при включении объекта
    /// </summary>
    private void OnEnable()
    {
        Initialize();
    }
    /// <summary>
    /// Поддерживает корректные значения и ссылки при изменениях в инспекторе Unity
    /// </summary>
    private void OnValidate()
    {
        if (target == null)
            target = transform as RectTransform;
    }
    /// <summary>
    /// Каждый кадр проверяет ввод или обновляет визуальное состояние
    /// </summary>
    private void Update()
    {
        Initialize();

        var desiredPosition = baseAnchoredPosition + (hovering ? Vector2.up * hoverLift : Vector2.zero);
        target.anchoredPosition = Vector2.SmoothDamp(target.anchoredPosition, desiredPosition, ref moveVelocity, moveSmoothTime);
    }
    /// <summary>
    /// Обрабатывает событие указателя мыши и переводит визуальный элемент в нужное состояние
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }
    /// <summary>
    /// Обрабатывает событие указателя мыши и переводит визуальный элемент в нужное состояние
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }
    /// <summary>
    /// Готовит систему к работе и заполняет недостающие ссылки
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
