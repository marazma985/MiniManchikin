using System.Collections.Generic;
using UnityEngine;

public sealed class CardHandView : MonoBehaviour
{
    [SerializeField] private CardSystem cardSystem;
    [SerializeField] private CardView[] cardViews;

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void Refresh()
    {
        Refresh(cardSystem != null ? cardSystem.Hand : null);
    }

    private void Subscribe()
    {
        if (cardSystem == null)
            return;

        cardSystem.OnHandChanged += Refresh;
    }

    private void Unsubscribe()
    {
        if (cardSystem == null)
            return;

        cardSystem.OnHandChanged -= Refresh;
    }

    private void Refresh(IReadOnlyList<CardData> cards)
    {
        if (cardViews == null)
            return;

        for (var i = 0; i < cardViews.Length; i++)
        {
            var cardView = cardViews[i];
            if (cardView == null)
                continue;

            cardView.Clicked -= HandleCardClicked;
            cardView.RemoveClicked -= HandleCardRemoveClicked;

            var card = cards != null && i < cards.Count ? cards[i] : null;
            cardView.SetCard(card);
            cardView.gameObject.SetActive(card != null);

            if (card != null)
            {
                cardView.Clicked += HandleCardClicked;
                cardView.RemoveClicked += HandleCardRemoveClicked;
            }
        }
    }

    private void HandleCardClicked(CardData card)
    {
        if (cardSystem != null)
            cardSystem.UseCard(card);
    }

    private void HandleCardRemoveClicked(CardData card)
    {
        if (cardSystem != null)
            cardSystem.RemoveCard(card);
    }
}
