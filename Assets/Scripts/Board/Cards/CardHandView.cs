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
    /// Подписывает отображение руки на изменения карт и сразу перерисовывает руку
    /// </summary>
    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }
    /// <summary>
    /// Отписывает отображение руки от событий карт
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
    /// Подписывает все видимые карты в руке на клики и удаление
    /// </summary>
    private void Subscribe()
    {
        if (cardSystem == null)
            return;

        cardSystem.OnHandChanged += Refresh;
    }
    /// <summary>
    /// Отписывает карты в руке от кликов и удаления
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
    /// Передает выбранную игроком карту в систему карт
    /// </summary>
    private void HandleCardClicked(CardData card)
    {
        if (cardSystem != null)
            cardSystem.UseCard(card);
    }
    /// <summary>
    /// Передает запрос игрока на удаление карты из руки
    /// </summary>
    private void HandleCardRemoveClicked(CardData card)
    {
        if (cardSystem != null)
            cardSystem.RemoveCard(card);
    }
}
