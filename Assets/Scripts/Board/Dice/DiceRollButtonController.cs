using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// Отвечает за механику или визуал кубика, связанные с DiceRollButtonController
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
    /// Заполняет стандартные ссылки при добавлении компонента в редакторе Unity
    /// </summary>
    private void Reset()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        turnSystem = FindAnyObjectByType<TurnSystem>();
    }
    /// <summary>
    /// Подписывается на события и обновляет визуальное состояние при включении объекта
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
    /// Отписывается от событий и останавливает временные процессы при выключении объекта
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
    /// Поддерживает корректные значения и ссылки при изменениях в инспекторе Unity
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
    /// Обновляет визуальное состояние в конце кадра после работы остальных систем
    /// </summary>
    private void LateUpdate()
    {
        if (button == null || wasInteractable == button.interactable)
            return;

        RefreshVisualState(false);
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода RequestRoll
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
    /// Обрабатывает событие от UI или другой игровой системы
    /// </summary>
    private void HandleTurnStateChanged(TurnState state)
    {
        RefreshButtonState();
    }
    /// <summary>
    /// Обрабатывает событие от UI или другой игровой системы
    /// </summary>
    private void HandleDiceRolled(int value)
    {
        Debug.Log($"Dice rolled: {value}");
    }
    /// <summary>
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
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
    /// Устанавливает новое значение и при необходимости обновляет связанные системы
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
    /// Обновляет отображение на основе текущих данных
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
    /// Обрабатывает событие указателя мыши и переводит визуальный элемент в нужное состояние
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        RefreshVisualState(false);
    }
    /// <summary>
    /// Обрабатывает событие указателя мыши и переводит визуальный элемент в нужное состояние
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        isPointerPressed = false;
        RefreshVisualState(false);
    }
    /// <summary>
    /// Обрабатывает событие указателя мыши и переводит визуальный элемент в нужное состояние
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
    /// Обрабатывает событие указателя мыши и переводит визуальный элемент в нужное состояние
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerPressed = false;
        StopReleaseVisualRefresh();
        releaseVisualRefresh = StartCoroutine(RefreshReleasedVisualNextFrame());
    }
    /// <summary>
    /// Реагирует на событие select
    /// </summary>
    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
        RefreshVisualState(false);
    }
    /// <summary>
    /// Реагирует на событие deselect
    /// </summary>
    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        RefreshVisualState(false);
    }
    /// <summary>
    /// Настраивает ссылки и параметры, которые нужны компоненту для работы
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
    /// Обновляет отображение на основе текущих данных
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
    /// Обновляет отображение на основе текущих данных
    /// </summary>
    private IEnumerator RefreshReleasedVisualNextFrame()
    {
        yield return null;

        releaseVisualRefresh = null;
        RefreshVisualState(false);
    }
    /// <summary>
    /// Останавливает текущий процесс, корутину или визуальный переход
    /// </summary>
    private void StopReleaseVisualRefresh()
    {
        if (releaseVisualRefresh == null)
            return;

        StopCoroutine(releaseVisualRefresh);
        releaseVisualRefresh = null;
    }
}
