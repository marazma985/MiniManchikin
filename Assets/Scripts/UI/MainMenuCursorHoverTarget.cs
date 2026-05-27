using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// Отвечает за часть игровой логики или интерфейса, связанную с MainMenuCursorHoverTarget
/// </summary>

public sealed class MainMenuCursorHoverTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private bool hovering;
    /// <summary>
    /// Обрабатывает событие указателя мыши и переводит визуальный элемент в нужное состояние
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
        if (MainMenuCursor.Instance != null)
            MainMenuCursor.Instance.SetHover(true);
    }
    /// <summary>
    /// Обрабатывает событие указателя мыши и переводит визуальный элемент в нужное состояние
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        if (MainMenuCursor.Instance != null)
        {
            MainMenuCursor.Instance.SetHover(false);
        }
    }
    /// <summary>
    /// Обрабатывает событие указателя мыши и переводит визуальный элемент в нужное состояние
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (MainMenuCursor.Instance != null)
            MainMenuCursor.Instance.SetPressed(true);
    }
    /// <summary>
    /// Обрабатывает событие указателя мыши и переводит визуальный элемент в нужное состояние
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (MainMenuCursor.Instance != null)
        {
            MainMenuCursor.Instance.SetPressed(false);
            MainMenuCursor.Instance.SetHover(hovering);
        }
    }
}
