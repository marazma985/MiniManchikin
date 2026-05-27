using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// Кнопка броска кубика на игровом поле, которая запускает ход игрока
/// </summary>

public sealed class DiceRollButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Button button;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite highlightedSprite;
    [SerializeField] private Sprite pressedSprite;
    [SerializeField] private Sprite disabledSprite;
    [SerializeField, Min(0f)] private float spriteFadeDuration = 0.2f;
    [SerializeField] private SmoothSpriteButton spriteTransition;
    [SerializeField] private TurnSystem turnSystem;
    [SerializeField] private BattleSystem battleSystem;

    private Coroutine releaseVisualRefresh;
    private bool isPointerOver;
    private bool isPointerPressed;
    private bool isSelected;
    private bool wasInteractable;
    /// <summary>
    /// Заполняет удобные значения по умолчанию при добавлении компонента в Unity
    /// </summary>
    private void Reset()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        turnSystem = FindAnyObjectByType<TurnSystem>();
    }
    /// <summary>
    /// Включает подписки и обновляет отображение, когда объект становится активным
    /// </summary>
    private void OnEnable()
    {
        if (button == null)
            button = GetComponent<Button>();

        ConfigureButtonVisuals();

        if (button != null)
            button.onClick.AddListener(RequestRoll);

        if (turnSystem != null)
        {
            turnSystem.StateChanged += HandleTurnStateChanged;
            turnSystem.DiceRolled += HandleDiceRolled;
            HandleTurnStateChanged(turnSystem.State);
        }

        if (battleSystem != null)
            battleSystem.BattleStateChanged += RefreshButtonState;

        RefreshButtonState();
        RefreshVisualState(true);
    }
    /// <summary>
    /// Отключает подписки и временные процессы, когда объект выключается
    /// </summary>
    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(RequestRoll);

        if (turnSystem != null)
        {
            turnSystem.StateChanged -= HandleTurnStateChanged;
            turnSystem.DiceRolled -= HandleDiceRolled;
        }

        if (battleSystem != null)
            battleSystem.BattleStateChanged -= RefreshButtonState;

        StopReleaseVisualRefresh();
        if (spriteTransition != null)
            spriteTransition.StopTransition();
    }
    /// <summary>
    /// Помогает держать настройки компонента корректными прямо в инспекторе Unity
    /// </summary>
    private void OnValidate()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (buttonImage == null)
            buttonImage = GetComponent<Image>();

        if (spriteTransition == null)
            spriteTransition = GetComponent<SmoothSpriteButton>();
    }
    /// <summary>
    /// Обновляет отображение в конце кадра, когда остальные системы уже сработали
    /// </summary>
    private void LateUpdate()
    {
        if (button == null || wasInteractable == button.interactable)
            return;

        RefreshVisualState(false);
    }
    /// <summary>
    /// Обрабатывает нажатие кнопки броска кубика
    /// </summary>
    public void RequestRoll()
    {
        if (battleSystem != null && battleSystem.IsBattleActive)
        {
            battleSystem.RollBattleDice();
            RefreshButtonState();
            return;
        }

        if (turnSystem == null)
            return;

        turnSystem.TryRollDice();
    }
    /// <summary>
    /// Обрабатывает действие игрока или событие другой системы
    /// </summary>
    private void HandleTurnStateChanged(TurnState state)
    {
        RefreshButtonState();
    }
    /// <summary>
    /// Обрабатывает действие игрока или событие другой системы
    /// </summary>
    private void HandleDiceRolled(int value)
    {
        Debug.Log($"Dice rolled: {value}");
    }
    /// <summary>
    /// Обновляет данные, чтобы экран и правила игры сразу учитывали изменение
    /// </summary>
    public void SetTurnSystem(TurnSystem newTurnSystem)
    {
        if (turnSystem != null && isActiveAndEnabled)
        {
            turnSystem.StateChanged -= HandleTurnStateChanged;
            turnSystem.DiceRolled -= HandleDiceRolled;
        }

        turnSystem = newTurnSystem;

        if (turnSystem != null && isActiveAndEnabled)
        {
            turnSystem.StateChanged += HandleTurnStateChanged;
            turnSystem.DiceRolled += HandleDiceRolled;
            HandleTurnStateChanged(turnSystem.State);
        }
    }
    /// <summary>
    /// Обновляет данные, чтобы экран и правила игры сразу учитывали изменение
    /// </summary>
    public void SetBattleSystem(BattleSystem newBattleSystem)
    {
        if (battleSystem != null && isActiveAndEnabled)
            battleSystem.BattleStateChanged -= RefreshButtonState;

        battleSystem = newBattleSystem;

        if (battleSystem != null && isActiveAndEnabled)
            battleSystem.BattleStateChanged += RefreshButtonState;

        RefreshButtonState();
    }
    /// <summary>
    /// Включает или блокирует кнопку кубика по состоянию хода
    /// </summary>
    private void RefreshButtonState()
    {
        if (button == null)
            return;

        if (battleSystem != null && battleSystem.IsBattleActive)
        {
            button.interactable = battleSystem.CanUseBattleDice;
            RefreshVisualState(false);
            return;
        }

        button.interactable = turnSystem != null && turnSystem.CanRoll;
        RefreshVisualState(false);
    }
    /// <summary>
    /// Реагирует на мышь игрока и меняет вид элемента при наведении или нажатии
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        RefreshVisualState(false);
    }
    /// <summary>
    /// Реагирует на мышь игрока и меняет вид элемента при наведении или нажатии
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        isPointerPressed = false;
        RefreshVisualState(false);
    }
    /// <summary>
    /// Реагирует на мышь игрока и меняет вид элемента при наведении или нажатии
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (button == null || !button.interactable)
            return;

        StopReleaseVisualRefresh();
        isPointerPressed = true;
        RefreshVisualState(false);
    }
    /// <summary>
    /// Реагирует на мышь игрока и меняет вид элемента при наведении или нажатии
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerPressed = false;
        StopReleaseVisualRefresh();
        releaseVisualRefresh = StartCoroutine(RefreshReleasedVisualNextFrame());
    }
    /// <summary>
    /// Отмечает кнопку кубика как выбранную через клавиатуру или EventSystem
    /// </summary>
    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
        RefreshVisualState(false);
    }
    /// <summary>
    /// Снимает выбранное состояние с кнопки кубика
    /// </summary>
    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        RefreshVisualState(false);
    }
    /// <summary>
    /// Настраивает визуальную часть кнопки кубика и плавную смену спрайтов
    /// </summary>
    private void ConfigureButtonVisuals()
    {
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();

        if (button == null)
            return;

        if (spriteTransition == null)
            spriteTransition = GetComponent<SmoothSpriteButton>();

        if (spriteTransition == null)
            spriteTransition = gameObject.AddComponent<SmoothSpriteButton>();

        spriteTransition.Configure(button, buttonImage, normalSprite, highlightedSprite, pressedSprite, disabledSprite, spriteFadeDuration);
        wasInteractable = button.interactable;
    }
    /// <summary>
    /// Обновляет внешний вид кнопки кубика по наведению, нажатию и блокировке
    /// </summary>
    private void RefreshVisualState(bool instant)
    {
        if (button == null)
            return;

        if (spriteTransition == null)
            ConfigureButtonVisuals();

        wasInteractable = button.interactable;
        if (spriteTransition != null)
            spriteTransition.SetInteractionState(isPointerOver, isPointerPressed, isSelected, instant);
    }
    /// <summary>
    /// Возвращает кнопку кубика в отпущенный вид на следующем кадре
    /// </summary>
    private IEnumerator RefreshReleasedVisualNextFrame()
    {
        yield return null;

        releaseVisualRefresh = null;
        RefreshVisualState(false);
    }
    /// <summary>
    /// Останавливает текущий процесс или анимацию
    /// </summary>
    private void StopReleaseVisualRefresh()
    {
        if (releaseVisualRefresh == null)
            return;

        StopCoroutine(releaseVisualRefresh);
        releaseVisualRefresh = null;
    }
}
