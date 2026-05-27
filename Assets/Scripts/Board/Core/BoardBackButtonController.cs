using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
/// <summary>
/// Отвечает за базовую механику игрового поля, связанную с BoardBackButtonController
/// </summary>

public sealed class BoardBackButtonController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer mainRenderer;
    [SerializeField] private SpriteRenderer transitionRenderer;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private Sprite pressedSprite;
    [SerializeField, Min(0f)] private float fadeDuration = 0.2f;
    [SerializeField] private SmoothSpriteRendererTransition spriteTransition;
    [SerializeField] private string targetSceneName = "MainMenu";
    [SerializeField] private Camera anchorCamera;
    [SerializeField] private Vector2 worldMargin = new Vector2(0.35f, 0.35f);
    [SerializeField] private GameObject[] blockingModalRoots;

    private bool isPointerOver;
    private bool isPointerPressed;
    /// <summary>
    /// Заполняет стандартные ссылки при добавлении компонента в редакторе Unity
    /// </summary>
    private void Reset()
    {
        mainRenderer = GetComponent<SpriteRenderer>();
        anchorCamera = Camera.main;
    }
    /// <summary>
    /// Инициализирует ссылки и внутреннее состояние до запуска сцены
    /// </summary>
    private void Awake()
    {
        if (mainRenderer == null)
            mainRenderer = GetComponent<SpriteRenderer>();

        if (anchorCamera == null)
            anchorCamera = Camera.main;

        ConfigureRenderers();
        AnchorToCamera();
    }
    /// <summary>
    /// Подписывается на события и обновляет визуальное состояние при включении объекта
    /// </summary>
    private void OnEnable()
    {
        RefreshVisualState(true);
    }
    /// <summary>
    /// Отписывается от событий и останавливает временные процессы при выключении объекта
    /// </summary>
    private void OnDisable()
    {
        if (spriteTransition != null)
            spriteTransition.StopTransition();

        isPointerOver = false;
        isPointerPressed = false;
    }
    /// <summary>
    /// Каждый кадр проверяет ввод или обновляет визуальное состояние
    /// </summary>
    private void Update()
    {
        AnchorToCamera();

        if (IsInteractionBlocked())
        {
            ResetInteractionState();
            return;
        }

        var pointerOverButton = IsPointerOverButton();
        if (isPointerOver != pointerOverButton)
        {
            isPointerOver = pointerOverButton;
            if (!isPointerOver)
                isPointerPressed = false;

            RefreshVisualState(false);
        }

        if (Input.GetMouseButtonDown(0) && isPointerOver)
        {
            isPointerPressed = true;
            RefreshVisualState(false);
        }

        if (!Input.GetMouseButtonUp(0))
            return;

        var shouldLoadScene = isPointerPressed && isPointerOver;
        isPointerPressed = false;
        RefreshVisualState(false);

        if (shouldLoadScene && !string.IsNullOrEmpty(targetSceneName))
        {
            if (GameSaveController.Instance != null)
                GameSaveController.Instance.SaveNowEvenIfInitializing();

            SceneManager.LoadScene(targetSceneName);
        }
    }
    /// <summary>
    /// Настраивает ссылки и параметры, которые нужны компоненту для работы
    /// </summary>
    private void ConfigureRenderers()
    {
        if (mainRenderer != null)
        {
            mainRenderer.sprite = normalSprite;
            mainRenderer.color = Color.white;
        }

        if (transitionRenderer == null)
        {
            var transitionTransform = transform.Find("Transition");
            if (transitionTransform != null)
                transitionRenderer = transitionTransform.GetComponent<SpriteRenderer>();
        }

        if (spriteTransition == null)
            spriteTransition = GetComponent<SmoothSpriteRendererTransition>();

        if (spriteTransition == null)
            spriteTransition = gameObject.AddComponent<SmoothSpriteRendererTransition>();

        spriteTransition.Configure(mainRenderer, transitionRenderer, normalSprite, hoverSprite, pressedSprite, fadeDuration);
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода AnchorToCamera
    /// </summary>
    private void AnchorToCamera()
    {
        if (anchorCamera == null || mainRenderer == null)
            return;

        var zDistance = Mathf.Abs(transform.position.z - anchorCamera.transform.position.z);
        var topRight = anchorCamera.ViewportToWorldPoint(new Vector3(1f, 1f, zDistance));
        topRight.z = transform.position.z;

        var targetMax = new Vector3(topRight.x - worldMargin.x, topRight.y - worldMargin.y, transform.position.z);
        var delta = targetMax - mainRenderer.bounds.max;
        delta.z = 0f;
        transform.position += delta;
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода IsInteractionBlocked
    /// </summary>
    private bool IsInteractionBlocked()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return true;

        if (blockingModalRoots == null)
            return false;

        foreach (var modalRoot in blockingModalRoots)
        {
            if (modalRoot != null && modalRoot.activeInHierarchy)
                return true;
        }

        return false;
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода IsPointerOverButton
    /// </summary>
    private bool IsPointerOverButton()
    {
        if (anchorCamera == null || mainRenderer == null)
            return false;

        var zDistance = Mathf.Abs(transform.position.z - anchorCamera.transform.position.z);
        var mousePosition = Input.mousePosition;
        mousePosition.z = zDistance;
        var worldPosition = anchorCamera.ScreenToWorldPoint(mousePosition);
        worldPosition.z = mainRenderer.bounds.center.z;

        return mainRenderer.bounds.Contains(worldPosition);
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода ResetInteractionState
    /// </summary>
    private void ResetInteractionState()
    {
        if (!isPointerOver && !isPointerPressed)
            return;

        isPointerOver = false;
        isPointerPressed = false;
        RefreshVisualState(false);
    }
    /// <summary>
    /// Обновляет отображение на основе текущих данных
    /// </summary>
    private void RefreshVisualState(bool instant)
    {
        if (spriteTransition == null)
            ConfigureRenderers();

        if (spriteTransition != null)
            spriteTransition.SetInteractionState(isPointerOver, isPointerPressed, instant);
    }
}
