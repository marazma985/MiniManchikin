using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    private void Reset()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        turnSystem = FindAnyObjectByType<TurnSystem>();
    }

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

    private void OnValidate()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (buttonImage == null)
            buttonImage = GetComponent<Image>();

        if (spriteTransition == null)
            spriteTransition = GetComponent<SmoothSpriteButton>();
    }

    private void LateUpdate()
    {
        if (button == null || wasInteractable == button.interactable)
            return;

        RefreshVisualState(false);
    }

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

    private void HandleTurnStateChanged(TurnState state)
    {
        RefreshButtonState();
    }

    private void HandleDiceRolled(int value)
    {
        Debug.Log($"Dice rolled: {value}");
    }

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

    public void SetBattleSystem(BattleSystem newBattleSystem)
    {
        if (battleSystem != null && isActiveAndEnabled)
            battleSystem.BattleStateChanged -= RefreshButtonState;

        battleSystem = newBattleSystem;

        if (battleSystem != null && isActiveAndEnabled)
            battleSystem.BattleStateChanged += RefreshButtonState;

        RefreshButtonState();
    }

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        RefreshVisualState(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        isPointerPressed = false;
        RefreshVisualState(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button == null || !button.interactable)
            return;

        StopReleaseVisualRefresh();
        isPointerPressed = true;
        RefreshVisualState(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerPressed = false;
        StopReleaseVisualRefresh();
        releaseVisualRefresh = StartCoroutine(RefreshReleasedVisualNextFrame());
    }

    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
        RefreshVisualState(false);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        RefreshVisualState(false);
    }

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

    private IEnumerator RefreshReleasedVisualNextFrame()
    {
        yield return null;

        releaseVisualRefresh = null;
        RefreshVisualState(false);
    }

    private void StopReleaseVisualRefresh()
    {
        if (releaseVisualRefresh == null)
            return;

        StopCoroutine(releaseVisualRefresh);
        releaseVisualRefresh = null;
    }
}
