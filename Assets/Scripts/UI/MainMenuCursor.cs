using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
/// <summary>
/// Показывает собственный курсор игры вместо стандартного курсора системы
/// </summary>

public sealed class MainMenuCursor : MonoBehaviour
{
    private const string GlobalCursorPrefabPath = "UI/GlobalCursor";
    /// <summary>
    /// Набор вариантов, из которых игра выбирает нужное состояние для CursorState
    /// </summary>
    public enum CursorState
    {
        Normal,
        Hover,
        Pressed
    }

    public static MainMenuCursor Instance { get; private set; }

    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform cursorTransform;
    [SerializeField] private Image cursorImage;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private Sprite pressedSprite;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Vector2 cursorSize = new Vector2(110f, 132f);
    [SerializeField] private Vector2 hotspotPivot = new Vector2(0.34f, 0.84f);

    private CursorState state = CursorState.Normal;
    private bool hoveringButton;
    private bool pressingButton;
    private readonly List<RaycastResult> pointerRaycastResults = new List<RaycastResult>();
    private EventSystem pointerEventSystem;
    private PointerEventData pointerEventData;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    /// <summary>
    /// Передает данные другой системе, чтобы она могла ими пользоваться
    /// </summary>
    private static void RegisterGlobalCursor()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        EnsureGlobalCursor();
    }
    /// <summary>
    /// Обрабатывает действие игрока или событие другой системы
    /// </summary>
    private static void HandleSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        EnsureGlobalCursor();
    }
    /// <summary>
    /// Создает или находит то, без чего объект не сможет работать
    /// </summary>
    private static void EnsureGlobalCursor()
    {
        if (Instance != null || FindAnyObjectByType<MainMenuCursor>() != null)
            return;

        var cursorPrefab = Resources.Load<GameObject>(GlobalCursorPrefabPath);
        if (cursorPrefab == null)
        {
            Debug.LogWarning($"Global cursor prefab is missing at Resources/{GlobalCursorPrefabPath}.");
            return;
        }

        Instantiate(cursorPrefab);
    }
    /// <summary>
    /// Находит картинку курсора и готовит ее к слежению за мышью
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
        ApplyState(CursorState.Normal);
    }
    /// <summary>
    /// Убирает ссылки и временные данные перед уничтожением объекта
    /// </summary>
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        Cursor.visible = true;
    }
    /// <summary>
    /// Сохраняет или обновляет состояние при потере и возврате фокуса приложения
    /// </summary>
    private void OnApplicationFocus(bool hasFocus)
    {
        Cursor.visible = !hasFocus ? true : false;
    }
    /// <summary>
    /// Каждый кадр проверяет ввод игрока или обновляет отображение
    /// </summary>
    private void Update()
    {
        FollowMouse();
        RefreshAutomaticUiState();

        if (!Input.GetMouseButton(0) && pressingButton)
            SetPressed(false);
    }
    /// <summary>
    /// Обновляет данные, чтобы экран и правила игры сразу учитывали изменение
    /// </summary>
    public void SetHover(bool isHovering)
    {
        hoveringButton = isHovering;
        if (!pressingButton)
            ApplyState(hoveringButton ? CursorState.Hover : CursorState.Normal);
    }
    /// <summary>
    /// Обновляет данные, чтобы экран и правила игры сразу учитывали изменение
    /// </summary>
    public void SetPressed(bool isPressed)
    {
        pressingButton = isPressed;
        ApplyState(pressingButton ? CursorState.Pressed : hoveringButton ? CursorState.Hover : CursorState.Normal);
    }
    /// <summary>
    /// Перемещает игровой курсор в позицию мыши
    /// </summary>
    private void FollowMouse()
    {
        if (canvas == null || cursorTransform == null)
            return;

        var canvasRect = (RectTransform)canvas.transform;
        var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, camera, out var localPosition))
            cursorTransform.anchoredPosition = localPosition;
    }
    /// <summary>
    /// Меняет вид курсора при наведении на кнопки и элементы интерфейса
    /// </summary>
    private void RefreshAutomaticUiState()
    {
        var hoveringSelectable = IsPointerOverSelectable();
        if (hoveringSelectable != hoveringButton)
            SetHover(hoveringSelectable);

        if (Input.GetMouseButtonDown(0))
            SetPressed(true);
        else if (Input.GetMouseButtonUp(0))
            SetPressed(false);
    }
    /// <summary>
    /// Проверяет, находится ли курсор над интерактивным UI-элементом
    /// </summary>
    private bool IsPointerOverSelectable()
    {
        var eventSystem = EventSystem.current;
        if (eventSystem == null)
            return false;

        if (pointerEventData == null || pointerEventSystem != eventSystem)
        {
            pointerEventSystem = eventSystem;
            pointerEventData = new PointerEventData(eventSystem);
        }

        pointerEventData.Reset();
        pointerEventData.position = Input.mousePosition;
        pointerRaycastResults.Clear();
        eventSystem.RaycastAll(pointerEventData, pointerRaycastResults);

        for (var i = 0; i < pointerRaycastResults.Count; i++)
        {
            var selectable = pointerRaycastResults[i].gameObject.GetComponentInParent<Selectable>();
            if (selectable != null && selectable.isActiveAndEnabled)
                return true;
        }

        return false;
    }
    /// <summary>
    /// Меняет спрайт курсора под обычное состояние или наведение
    /// </summary>
    private void ApplyState(CursorState nextState)
    {
        state = nextState;

        if (cursorTransform != null)
        {
            cursorTransform.pivot = hotspotPivot;
            cursorTransform.sizeDelta = cursorSize;
            cursorTransform.SetAsLastSibling();
        }

        if (cursorImage == null)
            return;

        cursorImage.sprite = PickSprite(state);
        cursorImage.color = Color.white;
        cursorImage.preserveAspect = true;
        cursorImage.raycastTarget = false;
    }
    /// <summary>
    /// Выбирает подходящий вариант
    /// </summary>
    private Sprite PickSprite(CursorState cursorState)
    {
        switch (cursorState)
        {
            case CursorState.Hover:
                return hoverSprite != null ? hoverSprite : normalSprite;
            case CursorState.Pressed:
                return pressedSprite != null ? pressedSprite : hoverSprite != null ? hoverSprite : normalSprite;
            default:
                return normalSprite;
        }
    }
}
