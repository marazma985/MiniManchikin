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
    [SerializeField] private TurnSystem turnSystem;
    [SerializeField] private BattleSystem battleSystem;

    private Image transitionImage;
    private Coroutine spriteFade;
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

        StopSpriteFade();
        StopReleaseVisualRefresh();
        HideTransitionImage();
        SetButtonAlpha(1f);
    }

    private void OnValidate()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (buttonImage == null)
            buttonImage = GetComponent<Image>();
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

        button.transition = Selectable.Transition.None;
        wasInteractable = button.interactable;

        if (buttonImage != null)
            buttonImage.color = Color.white;
    }

    private void RefreshVisualState(bool instant)
    {
        if (button == null || buttonImage == null)
            return;

        wasInteractable = button.interactable;
        SetButtonSprite(GetStateSprite(), instant);
    }

    private Sprite GetStateSprite()
    {
        if (button == null || !button.interactable)
            return disabledSprite != null ? disabledSprite : normalSprite;

        if (isPointerPressed && pressedSprite != null)
            return pressedSprite;

        if ((isPointerOver || isSelected) && highlightedSprite != null)
            return highlightedSprite;

        return normalSprite;
    }

    private void SetButtonSprite(Sprite targetSprite, bool instant)
    {
        if (targetSprite == null)
            return;

        if (buttonImage.sprite == targetSprite)
        {
            if (instant)
            {
                StopSpriteFade();
                SetButtonAlpha(1f);
            }

            return;
        }

        var previousSprite = buttonImage.sprite;
        buttonImage.sprite = targetSprite;
        SetButtonAlpha(1f);

        if (instant || previousSprite == null || spriteFadeDuration <= 0f)
        {
            StopSpriteFade();
            HideTransitionImage();
            return;
        }

        EnsureTransitionImage();
        if (transitionImage == null)
            return;

        StopSpriteFade();
        transitionImage.sprite = previousSprite;
        transitionImage.color = Color.white;
        transitionImage.enabled = true;
        spriteFade = StartCoroutine(FadePreviousSprite());
    }

    private void EnsureTransitionImage()
    {
        if (transitionImage != null || buttonImage == null)
            return;

        var transitionObject = new GameObject("Roll Dice Sprite Transition", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var transitionTransform = transitionObject.GetComponent<RectTransform>();
        transitionTransform.SetParent(buttonImage.rectTransform, false);
        transitionTransform.anchorMin = Vector2.zero;
        transitionTransform.anchorMax = Vector2.one;
        transitionTransform.pivot = buttonImage.rectTransform.pivot;
        transitionTransform.anchoredPosition = Vector2.zero;
        transitionTransform.sizeDelta = Vector2.zero;
        transitionTransform.localScale = Vector3.one;
        transitionTransform.localRotation = Quaternion.identity;
        transitionTransform.SetAsLastSibling();

        transitionImage = transitionObject.GetComponent<Image>();
        transitionImage.raycastTarget = false;
        transitionImage.type = buttonImage.type;
        transitionImage.preserveAspect = buttonImage.preserveAspect;
        transitionImage.pixelsPerUnitMultiplier = buttonImage.pixelsPerUnitMultiplier;
        transitionImage.enabled = false;
    }

    private IEnumerator FadePreviousSprite()
    {
        var elapsed = 0f;

        while (elapsed < spriteFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var progress = Mathf.Clamp01(elapsed / spriteFadeDuration);
            transitionImage.color = new Color(1f, 1f, 1f, 1f - progress);
            yield return null;
        }

        HideTransitionImage();
        spriteFade = null;
    }

    private void SetButtonAlpha(float alpha)
    {
        if (buttonImage == null)
            return;

        buttonImage.color = new Color(1f, 1f, 1f, alpha);
    }

    private void HideTransitionImage()
    {
        if (transitionImage == null)
            return;

        transitionImage.sprite = null;
        transitionImage.enabled = false;
    }

    private void StopSpriteFade()
    {
        if (spriteFade == null)
            return;

        StopCoroutine(spriteFade);
        spriteFade = null;
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
