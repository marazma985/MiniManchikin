using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Отвечает за работу карт и логику, связанную с CardHandView
/// </summary>

public sealed class CardHandView : MonoBehaviour
{
    [SerializeField] private CardSystem cardSystem;
    [SerializeField] private CardView[] cardViews;

    /// <summary>
    /// Подписывается на изменения руки и сразу обновляет слоты карт
    /// </summary>
    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }
    /// <summary>
    /// Отписывается от событий и останавливает временные процессы при выключении объекта
    /// </summary>
    private void OnDisable()
    {
        Unsubscribe();
    }
    /// <summary>
    /// Обновляет отображение на основе текущих данных
    /// </summary>
    public void Refresh()
    {
        Refresh(cardSystem != null ? cardSystem.Hand : null);
    }
    /// <summary>
    /// Подписывает компонент на события зависимых систем
    /// </summary>
    private void Subscribe()
    {
        if (cardSystem == null)
            return;

        cardSystem.OnHandChanged += Refresh;
    }
    /// <summary>
    /// Снимает подписки, чтобы не оставить устаревшие ссылки
    /// </summary>
    private void Unsubscribe()
    {
        if (cardSystem == null)
            return;

        cardSystem.OnHandChanged -= Refresh;
    }
    /// <summary>
    /// Обновляет отображение на основе текущих данных
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
    /// Обрабатывает событие от UI или другой игровой системы
    /// </summary>
    private void HandleCardClicked(CardData card)
    {
        if (cardSystem != null)
            cardSystem.UseCard(card);
    }
    /// <summary>
    /// Обрабатывает событие от UI или другой игровой системы
    /// </summary>
    private void HandleCardRemoveClicked(CardData card)
    {
        if (cardSystem != null)
            cardSystem.RemoveCard(card);
    }
}
