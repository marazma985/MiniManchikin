using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Показывает карты в руке игрока и обновляет их, когда рука меняется
/// </summary>

public sealed class CardHandView : MonoBehaviour
{
    [SerializeField] private CardSystem cardSystem;
    [SerializeField] private CardView[] cardViews;

    /// <summary>
    /// Включает подписки и обновляет отображение, когда объект становится активным
    /// </summary>
    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }
    /// <summary>
    /// Отключает подписки и временные процессы, когда объект выключается
    /// </summary>
    private void OnDisable()
    {
        Unsubscribe();
    }
    /// <summary>
    /// Перерисовывает карты в руке игрока
    /// </summary>
    public void Refresh()
    {
        Refresh(cardSystem != null ? cardSystem.Hand : null);
    }
    /// <summary>
    /// Подписывается на события другой системы
    /// </summary>
    private void Subscribe()
    {
        if (cardSystem == null)
            return;

        cardSystem.OnHandChanged += Refresh;
    }
    /// <summary>
    /// Отписывается от событий другой системы
    /// </summary>
    private void Unsubscribe()
    {
        if (cardSystem == null)
            return;

        cardSystem.OnHandChanged -= Refresh;
    }
    /// <summary>
    /// Перерисовывает карты в руке игрока
    /// </summary>
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
    /// <summary>
    /// Обрабатывает действие игрока или событие другой системы
    /// </summary>
    private void HandleCardClicked(CardData card)
    {
        if (cardSystem != null)
            cardSystem.UseCard(card);
    }
    /// <summary>
    /// Обрабатывает действие игрока или событие другой системы
    /// </summary>
    private void HandleCardRemoveClicked(CardData card)
    {
        if (cardSystem != null)
            cardSystem.RemoveCard(card);
    }
}
