using System.Collections;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Создает затемненный и размытый фон за окном боя, чтобы внимание игрока было на модальном окне
/// </summary>

public sealed class BattleBackgroundBlurView : MonoBehaviour
{
    [SerializeField] private Camera sourceCamera;
    [SerializeField] private Material blurMaterial;
    [SerializeField] private RawImage blurImage;
    [SerializeField, Min(64)] private int captureWidth = 640;
    [SerializeField, Min(64)] private int captureHeight = 360;
    [SerializeField] private Color backdropColor = new Color(1f, 1f, 1f, 0.86f);

    private RenderTexture blurTexture;
    private Coroutine captureRoutine;
    /// <summary>
    /// Передает камеру и материал, из которых будет собран размытый фон окна боя
    /// </summary>
    public void Configure(Camera newSourceCamera, Material newBlurMaterial)
    {
        sourceCamera = newSourceCamera;
        blurMaterial = newBlurMaterial;
    }
    /// <summary>
    /// Включает подписки и обновляет отображение, когда объект становится активным
    /// </summary>
    private void OnEnable()
    {
        EnsureBlurImage();
        captureRoutine = StartCoroutine(CaptureBackgroundAtEndOfFrame());
    }
    /// <summary>
    /// Отключает подписки и временные процессы, когда объект выключается
    /// </summary>
    private void OnDisable()
    {
        if (captureRoutine != null)
        {
            StopCoroutine(captureRoutine);
            captureRoutine = null;
        }

        ReleaseBlurTexture();
    }
    /// <summary>
    /// Помогает держать настройки компонента корректными прямо в инспекторе Unity
    /// </summary>
    private void OnValidate()
    {
        captureWidth = Mathf.Max(64, captureWidth);
        captureHeight = Mathf.Max(64, captureHeight);
    }
    /// <summary>
    /// Создает или находит то, без чего объект не сможет работать
    /// </summary>
    private void EnsureBlurImage()
    {
        if (blurImage == null)
        {
            var imageObject = new GameObject("Battle Blur Backdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            imageObject.layer = gameObject.layer;
            imageObject.transform.SetParent(transform, false);
            imageObject.transform.SetAsFirstSibling();

            blurImage = imageObject.GetComponent<RawImage>();
            blurImage.raycastTarget = false;

            var rectTransform = imageObject.transform as RectTransform;
            if (rectTransform != null)
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }
        }

        blurImage.material = blurMaterial;
        blurImage.color = blurImage.texture != null ? backdropColor : Color.clear;
        blurImage.gameObject.SetActive(true);
        blurImage.transform.SetAsFirstSibling();
    }
    /// <summary>
    /// Запоминает текущее состояние, чтобы его можно было сохранить
    /// </summary>
    private void CaptureBackground()
    {
        if (sourceCamera == null)
        {
            Debug.LogWarning("BattleBackgroundBlurView requires a source camera.");
            return;
        }

        EnsureRenderTexture();

        var previousTargetTexture = sourceCamera.targetTexture;
        var previousActiveTexture = RenderTexture.active;

        sourceCamera.targetTexture = blurTexture;
        RenderTexture.active = blurTexture;
        sourceCamera.Render();
        sourceCamera.targetTexture = previousTargetTexture;
        RenderTexture.active = previousActiveTexture;

        if (blurImage != null)
        {
            blurImage.texture = blurTexture;
            blurImage.color = backdropColor;
        }
    }
    /// <summary>
    /// Запоминает текущее состояние, чтобы его можно было сохранить
    /// </summary>
    private IEnumerator CaptureBackgroundAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        CaptureBackground();
        captureRoutine = null;
    }
    /// <summary>
    /// Создает или находит то, без чего объект не сможет работать
    /// </summary>
    private void EnsureRenderTexture()
    {
        if (blurTexture != null && blurTexture.width == captureWidth && blurTexture.height == captureHeight)
            return;

        ReleaseBlurTexture();

        blurTexture = new RenderTexture(captureWidth, captureHeight, 16, RenderTextureFormat.ARGB32)
        {
            name = "Battle Background Blur Capture",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        blurTexture.Create();
    }
    /// <summary>
    /// Освобождает временные ресурсы
    /// </summary>
    private void ReleaseBlurTexture()
    {
        if (blurImage != null && blurImage.texture == blurTexture)
            blurImage.texture = null;

        if (blurTexture == null)
            return;

        blurTexture.Release();

        if (Application.isPlaying)
            Destroy(blurTexture);
        else
            DestroyImmediate(blurTexture);

        blurTexture = null;
    }
}
