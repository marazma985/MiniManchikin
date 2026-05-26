using UnityEngine;
using UnityEngine.EventSystems;

public sealed class MainMenuCursorHoverTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private bool hovering;

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
        if (MainMenuCursor.Instance != null)
            MainMenuCursor.Instance.SetHover(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        if (MainMenuCursor.Instance != null)
        {
            MainMenuCursor.Instance.SetHover(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (MainMenuCursor.Instance != null)
            MainMenuCursor.Instance.SetPressed(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (MainMenuCursor.Instance != null)
        {
            MainMenuCursor.Instance.SetPressed(false);
            MainMenuCursor.Instance.SetHover(hovering);
        }
    }
}
