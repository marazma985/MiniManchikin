using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// Сообщает игровому курсору, когда мышь наведена на кнопку или нажимает ее
/// </summary>

public sealed class MainMenuCursorHoverTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private bool hovering;
    /// <summary>
    /// Реагирует на мышь игрока и меняет вид элемента при наведении или нажатии
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
        if (MainMenuCursor.Instance != null)
            MainMenuCursor.Instance.SetHover(true);
    }
    /// <summary>
    /// Реагирует на мышь игрока и меняет вид элемента при наведении или нажатии
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
    /// Реагирует на мышь игрока и меняет вид элемента при наведении или нажатии
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (MainMenuCursor.Instance != null)
            MainMenuCursor.Instance.SetPressed(true);
    }
    /// <summary>
    /// Реагирует на мышь игрока и меняет вид элемента при наведении или нажатии
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
