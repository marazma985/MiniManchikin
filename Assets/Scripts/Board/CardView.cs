using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class CardView : MonoBehaviour
{
    [SerializeField] private Image cardImage;
    [SerializeField] private Button button;

    private CardData currentCard;

    public event Action<CardData> Clicked;

    public CardData CurrentCard => currentCard;

    private void OnEnable()
    {
        if (button != null)
            button.onClick.AddListener(HandleClick);

        RefreshVisual();
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);
    }

    public void SetCard(CardData card)
    {
        currentCard = card;
        RefreshVisual();
    }

    private void RefreshVisual()
    {
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
}
