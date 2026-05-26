using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public sealed class BoardBackButtonController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer mainRenderer;
    [SerializeField] private SpriteRenderer transitionRenderer;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private Sprite pressedSprite;
    [SerializeField, Min(0f)] private float fadeDuration = 0.2f;
    [SerializeField] private string targetSceneName = "MainMenu";
    [SerializeField] private Camera anchorCamera;
    [SerializeField] private Vector2 worldMargin = new Vector2(0.35f, 0.35f);
    [SerializeField] private GameObject[] blockingModalRoots;

    private Coroutine fadeCoroutine;
    private bool isPointerOver;
    private bool isPointerPressed;

    private void Reset()
    {
        mainRenderer = GetComponent<SpriteRenderer>();
        anchorCamera = Camera.main;
    }

    private void Awake()
    {
        if (mainRenderer == null)
            mainRenderer = GetComponent<SpriteRenderer>();

        if (anchorCamera == null)
            anchorCamera = Camera.main;

        ConfigureRenderers();
        SetButtonSprite(normalSprite, true);
        AnchorToCamera();
    }

    private void OnEnable()
    {
        RefreshVisualState(true);
    }

    private void OnDisable()
    {
        StopFade();
        HideTransitionRenderer();
        isPointerOver = false;
        isPointerPressed = false;
    }

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
            SceneManager.LoadScene(targetSceneName);
    }

    private void ConfigureRenderers()
    {
        if (mainRenderer != null)
        {
            mainRenderer.sprite = normalSprite;
            mainRenderer.color = Color.white;
        }

        if (transitionRenderer == null)
            return;

        transitionRenderer.sprite = null;
        transitionRenderer.enabled = false;

        if (mainRenderer != null)
        {
            transitionRenderer.sortingLayerID = mainRenderer.sortingLayerID;
            transitionRenderer.sortingOrder = mainRenderer.sortingOrder + 1;
        }
    }

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

    private void ResetInteractionState()
    {
        if (!isPointerOver && !isPointerPressed)
            return;

        isPointerOver = false;
        isPointerPressed = false;
        RefreshVisualState(false);
    }

    private void RefreshVisualState(bool instant)
    {
        SetButtonSprite(GetStateSprite(), instant);
    }

    private Sprite GetStateSprite()
    {
        if (isPointerPressed && pressedSprite != null)
            return pressedSprite;

        if (isPointerOver && hoverSprite != null)
            return hoverSprite;

        return normalSprite;
    }

    private void SetButtonSprite(Sprite targetSprite, bool instant)
    {
        if (mainRenderer == null || targetSprite == null)
            return;

        if (mainRenderer.sprite == targetSprite)
        {
            if (instant)
            {
                StopFade();
                SetMainAlpha(1f);
                HideTransitionRenderer();
            }

            return;
        }

        var previousSprite = mainRenderer.sprite;
        mainRenderer.sprite = targetSprite;
        SetMainAlpha(1f);

        if (instant || previousSprite == null || transitionRenderer == null || fadeDuration <= 0f)
        {
            StopFade();
            HideTransitionRenderer();
            return;
        }

        StopFade();
        transitionRenderer.sprite = previousSprite;
        transitionRenderer.color = Color.white;
        transitionRenderer.enabled = true;
        fadeCoroutine = StartCoroutine(FadePreviousSprite());
    }

    private IEnumerator FadePreviousSprite()
    {
        var elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var progress = Mathf.Clamp01(elapsed / fadeDuration);
            transitionRenderer.color = new Color(1f, 1f, 1f, 1f - progress);
            yield return null;
        }

        HideTransitionRenderer();
        fadeCoroutine = null;
    }

    private void SetMainAlpha(float alpha)
    {
        if (mainRenderer == null)
            return;

        mainRenderer.color = new Color(1f, 1f, 1f, alpha);
    }

    private void HideTransitionRenderer()
    {
        if (transitionRenderer == null)
            return;

        transitionRenderer.sprite = null;
        transitionRenderer.enabled = false;
    }

    private void StopFade()
    {
        if (fadeCoroutine == null)
            return;

        StopCoroutine(fadeCoroutine);
        fadeCoroutine = null;
    }
}
