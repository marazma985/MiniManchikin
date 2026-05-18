using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class CardView : MonoBehaviour
{
    [SerializeField] private Image cardImage;
    [SerializeField] private Button button;
    [SerializeField] private Button removeButton;

    private CardData currentCard;

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
}
