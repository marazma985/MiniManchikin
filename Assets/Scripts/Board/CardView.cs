using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image cardImage;
    [SerializeField] private Button button;
    [SerializeField] private Button removeButton;
    [SerializeField] private CanvasGroup removeButtonCanvasGroup;
    [SerializeField, Min(0f)] private float removeButtonFadeDuration = 0.15f;

    private CardData currentCard;
    private Coroutine removeButtonFade;
    private bool isPointerOver;

    public event Action<CardData> Clicked;
    public event Action<CardData> RemoveClicked;

    public CardData CurrentCard => currentCard;

    private void OnEnable()
    {
        if (button != null)
            button.onClick.AddListener(HandleClick);

        if (removeButton != null)
            removeButton.onClick.AddListener(HandleRemoveClick);

        RefreshVisual();
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);

        if (removeButton != null)
            removeButton.onClick.RemoveListener(HandleRemoveClick);

        StopRemoveButtonFade();
    }

    public void SetCard(CardData card)
    {
        currentCard = card;
        RefreshVisual();
    }

    private void RefreshVisual()
    {
        if (removeButton != null)
            removeButton.gameObject.SetActive(currentCard != null);

        SetRemoveButtonVisible(currentCard != null && isPointerOver, true);

        if (cardImage == null)
            return;

        var sprite = currentCard != null ? currentCard.CardSprite : null;
        cardImage.sprite = sprite;
        cardImage.enabled = sprite != null;
    }

    private void HandleClick()
    {
        if (currentCard != null)
            Clicked?.Invoke(currentCard);
    }

    private void HandleRemoveClick()
    {
        if (currentCard != null)
            RemoveClicked?.Invoke(currentCard);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        SetRemoveButtonVisible(currentCard != null, false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        SetRemoveButtonVisible(false, false);
    }

    private void SetRemoveButtonVisible(bool visible, bool instant)
    {
        if (removeButtonCanvasGroup == null)
            return;

        StopRemoveButtonFade();
        removeButtonCanvasGroup.interactable = visible;
        removeButtonCanvasGroup.blocksRaycasts = visible;

        if (instant || removeButtonFadeDuration <= 0f)
        {
            removeButtonCanvasGroup.alpha = visible ? 1f : 0f;
            return;
        }

        removeButtonFade = StartCoroutine(FadeRemoveButton(visible ? 1f : 0f));
    }

    private IEnumerator FadeRemoveButton(float targetAlpha)
    {
        var startAlpha = removeButtonCanvasGroup.alpha;
        var elapsed = 0f;

        while (elapsed < removeButtonFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var progress = Mathf.Clamp01(elapsed / removeButtonFadeDuration);
            removeButtonCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            yield return null;
        }

        removeButtonCanvasGroup.alpha = targetAlpha;
        removeButtonFade = null;
    }

    private void StopRemoveButtonFade()
    {
        if (removeButtonFade == null)
            return;

        StopCoroutine(removeButtonFade);
        removeButtonFade = null;
    }
}
