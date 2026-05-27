using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Модальное окно настроек в главном меню, где игрок меняет окно, полный экран и громкость
/// </summary>

public sealed class MainMenuSettingsModalView : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Camera sourceCamera;
    [SerializeField] private Sprite modalSprite;
    [SerializeField] private Material blurMaterial;
    [SerializeField] private GameObject modalRoot;
    [SerializeField] private GameObject inputBlockerObject;
    [SerializeField] private GameObject panelObject;
    [SerializeField] private GameSettingsService settingsService;
    [SerializeField] private Button openButton;
    [SerializeField] private Dropdown resolutionDropdown;
    [SerializeField] private Button resolutionButton;
    [SerializeField] private Text resolutionCaptionText;
    [SerializeField] private GameObject resolutionOptionsRoot;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Button saveButton;

    private readonly List<Text> resolutionOptionTexts = new List<Text>();
    private Coroutine showRoutine;
    private int selectedResolutionIndex = 1;
    /// <summary>
    /// Собирает окно настроек главного меню и заполняет его текущими значениями
    /// </summary>
    private void Awake()
    {
        EnsureUi();
        PopulateResolutionDropdown();
        Hide();
    }
    /// <summary>
    /// Подписывает кнопки и поля окна настроек на действия игрока
    /// </summary>
    private void OnEnable()
    {
        if (openButton != null)
            openButton.onClick.AddListener(Show);

        if (saveButton != null)
            saveButton.onClick.AddListener(Save);

    }
    /// <summary>
    /// Отписывает окно настроек от кнопок и полей ввода
    /// </summary>
    private void OnDisable()
    {
        if (openButton != null)
            openButton.onClick.RemoveListener(Show);

        if (saveButton != null)
            saveButton.onClick.RemoveListener(Save);

    }
    /// <summary>
    /// Открывает окно настроек поверх главного меню
    /// </summary>
    public void Show()
    {
        if (settingsService != null)
        {
            PopulateResolutionDropdown();

            SetSelectedResolutionIndex(settingsService.GetSavedResolutionIndex(), false);

            if (fullscreenToggle != null)
                fullscreenToggle.isOn = settingsService.GetSavedFullscreen();

            if (musicVolumeSlider != null)
                musicVolumeSlider.value = settingsService.GetSavedMusicVolume();
        }

        if (modalRoot != null)
        {
            if (showRoutine != null)
                StopCoroutine(showRoutine);

            showRoutine = StartCoroutine(ShowAfterBlurCapture());
        }
    }
    /// <summary>
    /// Закрывает окно настроек и возвращает фокус главному меню
    /// </summary>
    public void Hide()
    {
        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
            showRoutine = null;
        }

        if (modalRoot != null)
            modalRoot.SetActive(false);

        if (resolutionOptionsRoot != null)
            resolutionOptionsRoot.SetActive(false);
    }
    /// <summary>
    /// Сохраняет данные партии или настроек
    /// </summary>
    private void Save()
    {
        if (settingsService != null)
        {
            var resolutionIndex = selectedResolutionIndex;
            var fullscreen = fullscreenToggle != null ? fullscreenToggle.isOn : settingsService.GetSavedFullscreen();
            var musicVolume = musicVolumeSlider != null ? musicVolumeSlider.value : settingsService.GetSavedMusicVolume();

            settingsService.SaveAndApply(resolutionIndex, fullscreen, musicVolume);
        }

        Hide();
    }
    /// <summary>
    /// Заполняет список доступных размеров окна в настройках
    /// </summary>
    private void PopulateResolutionDropdown()
    {
        if (resolutionCaptionText == null || settingsService == null)
            return;

        var options = new List<string>();
        for (var i = 0; i < settingsService.ResolutionCount; i++)
            options.Add(settingsService.GetResolutionLabel(i));

        for (var i = 0; i < resolutionOptionTexts.Count; i++)
            resolutionOptionTexts[i].text = i < options.Count ? options[i] : string.Empty;

        SetSelectedResolutionIndex(selectedResolutionIndex, false);
    }
    /// <summary>
    /// Создает недостающие элементы окна настроек прямо в сцене
    /// </summary>
    private void EnsureUi()
    {
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (settingsService == null)
            settingsService = GetComponent<GameSettingsService>();

        if (sourceCamera == null && canvas != null)
            sourceCamera = canvas.worldCamera;

        if (modalRoot != null)
            return;

        // Окно настроек создается кодом, поэтому в сцене не нужно вручную собирать все его дочерние элементы
        var parent = canvas != null ? canvas.transform : transform;
        modalRoot = CreateRect("Settings Modal", parent, Vector2.zero, Vector2.zero).gameObject;
        Stretch(modalRoot.transform as RectTransform);
        modalRoot.SetActive(false);

        var blurView = modalRoot.AddComponent<BattleBackgroundBlurView>();
        blurView.Configure(sourceCamera, blurMaterial);

        var blocker = CreateImage("Input Blocker", modalRoot.transform, null);
        inputBlockerObject = blocker.gameObject;
        Stretch(blocker.rectTransform);
        blocker.color = Color.clear;
        blocker.raycastTarget = true;

        var panel = CreateImage("Settings Panel", modalRoot.transform, modalSprite);
        panelObject = panel.gameObject;
        SetCentered(panel.rectTransform, Vector2.zero, new Vector2(500f, 500f));
        panel.raycastTarget = true;

        CreateText("Title", panel.transform, "Настройки", new Vector2(0f, 180f), new Vector2(380f, 54f), 32, TextAnchor.MiddleCenter, FontStyle.Bold);

        CreateText("Resolution Label", panel.transform, "Размер окна", new Vector2(-110f, 105f), new Vector2(170f, 36f), 22, TextAnchor.MiddleLeft, FontStyle.Normal);
        CreateResolutionSelector(panel.transform, new Vector2(100f, 105f), new Vector2(190f, 42f));

        CreateText("Fullscreen Label", panel.transform, "Режим на весь экран", new Vector2(-70f, 35f), new Vector2(250f, 36f), 22, TextAnchor.MiddleLeft, FontStyle.Normal);
        fullscreenToggle = CreateToggle("Fullscreen Toggle", panel.transform, new Vector2(160f, 35f), new Vector2(48f, 48f));

        CreateText("Music Volume Label", panel.transform, "Громкость музыки", new Vector2(0f, -40f), new Vector2(340f, 36f), 22, TextAnchor.MiddleCenter, FontStyle.Normal);
        musicVolumeSlider = CreateSlider("Music Volume Slider", panel.transform, new Vector2(0f, -90f), new Vector2(320f, 34f));
        musicVolumeSlider.minValue = 0f;
        musicVolumeSlider.maxValue = 1f;

        saveButton = CreateButton("Save Button", panel.transform, "Сохранить", new Vector2(0f, -175f), new Vector2(220f, 58f));
    }
    /// <summary>
    /// Делает снимок фона для размытия и затем показывает окно настроек
    /// </summary>
    private IEnumerator ShowAfterBlurCapture()
    {
        if (inputBlockerObject != null)
            inputBlockerObject.SetActive(false);

        if (panelObject != null)
            panelObject.SetActive(false);

        modalRoot.SetActive(true);

        yield return new WaitForEndOfFrame();
        yield return null;

        if (inputBlockerObject != null)
            inputBlockerObject.SetActive(true);

        if (panelObject != null)
            panelObject.SetActive(true);

        showRoutine = null;
    }
    /// <summary>
    /// Обновляет подпись выбранного размера окна в настройках
    /// </summary>
    private void UpdateResolutionCaption(int optionIndex)
    {
        if (settingsService == null || resolutionCaptionText == null)
            return;

        var clampedIndex = Mathf.Clamp(optionIndex, 0, settingsService.ResolutionCount - 1);
        resolutionCaptionText.text = settingsService.GetResolutionLabel(clampedIndex);
        resolutionCaptionText.color = Color.white;
    }
    /// <summary>
    /// Создает выпадающий список размеров окна в настройках
    /// </summary>
    private void CreateResolutionSelector(Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        resolutionOptionTexts.Clear();

        // Здесь создается собственный список размеров окна в стиле кнопок главного меню
        resolutionButton = CreateButtonSurface("Resolution Dropdown", parent, new Color(0.16f, 0.24f, 0.28f, 0.96f), anchoredPosition, size);
        resolutionButton.onClick.AddListener(ToggleResolutionOptions);

        resolutionCaptionText = CreateText("Label", resolutionButton.transform, string.Empty, Vector2.zero, new Vector2(size.x - 44f, size.y), 22, TextAnchor.MiddleLeft, FontStyle.Bold);
        resolutionCaptionText.color = Color.white;
        resolutionCaptionText.rectTransform.anchorMin = Vector2.zero;
        resolutionCaptionText.rectTransform.anchorMax = Vector2.one;
        resolutionCaptionText.rectTransform.offsetMin = new Vector2(12f, 0f);
        resolutionCaptionText.rectTransform.offsetMax = new Vector2(-36f, 0f);

        var arrow = CreateText("Arrow", resolutionButton.transform, "v", new Vector2(size.x * 0.5f - 18f, 0f), new Vector2(24f, size.y), 18, TextAnchor.MiddleCenter, FontStyle.Bold);
        arrow.color = Color.white;

        var optionsImage = CreateImage("Resolution Options", resolutionButton.transform, null);
        optionsImage.color = new Color(0.16f, 0.24f, 0.28f, 1f);
        optionsImage.raycastTarget = true;
        resolutionOptionsRoot = optionsImage.gameObject;

        var optionsRect = optionsImage.rectTransform;
        optionsRect.anchorMin = new Vector2(0f, 0f);
        optionsRect.anchorMax = new Vector2(1f, 0f);
        optionsRect.pivot = new Vector2(0.5f, 1f);
        optionsRect.anchoredPosition = new Vector2(0f, -2f);
        optionsRect.sizeDelta = new Vector2(0f, 126f);

        for (var i = 0; i < 3; i++)
        {
            // Так каждая кнопка размера окна запоминает именно свой вариант
            var optionIndex = i;
            var optionButton = CreateButtonSurface($"Resolution Option {i + 1}", optionsImage.transform, new Color(0.22f, 0.32f, 0.36f, 1f), Vector2.zero, new Vector2(size.x, 42f));
            var optionRect = optionButton.transform as RectTransform;
            optionRect.anchorMin = new Vector2(0f, 1f);
            optionRect.anchorMax = new Vector2(1f, 1f);
            optionRect.pivot = new Vector2(0.5f, 1f);
            optionRect.anchoredPosition = new Vector2(0f, -42f * i);
            optionRect.sizeDelta = new Vector2(0f, 42f);
            optionButton.onClick.AddListener(() => SetSelectedResolutionIndex(optionIndex, true));

            var optionText = CreateText("Label", optionButton.transform, string.Empty, Vector2.zero, new Vector2(size.x - 24f, 42f), 21, TextAnchor.MiddleLeft, FontStyle.Bold);
            optionText.color = Color.white;
            optionText.rectTransform.anchorMin = Vector2.zero;
            optionText.rectTransform.anchorMax = Vector2.one;
            optionText.rectTransform.offsetMin = new Vector2(6f, 0f);
            optionText.rectTransform.offsetMax = new Vector2(-6f, 0f);
            resolutionOptionTexts.Add(optionText);
        }

        resolutionOptionsRoot.SetActive(false);
    }
    /// <summary>
    /// Открывает или закрывает список размеров окна
    /// </summary>
    private void ToggleResolutionOptions()
    {
        if (resolutionOptionsRoot == null)
            return;

        var showOptions = !resolutionOptionsRoot.activeSelf;
        if (showOptions)
        {
            resolutionButton.transform.SetAsLastSibling();
            resolutionOptionsRoot.transform.SetAsLastSibling();
        }

        resolutionOptionsRoot.SetActive(showOptions);
    }
    /// <summary>
    /// Запоминает выбранный размер окна и обновляет подпись
    /// </summary>
    private void SetSelectedResolutionIndex(int optionIndex, bool hideOptions)
    {
        selectedResolutionIndex = settingsService != null
            ? Mathf.Clamp(optionIndex, 0, settingsService.ResolutionCount - 1)
            : Mathf.Clamp(optionIndex, 0, 2);

        UpdateResolutionCaption(selectedResolutionIndex);

        if (hideOptions && resolutionOptionsRoot != null)
            resolutionOptionsRoot.SetActive(false);
    }
    /// <summary>
    /// Создает RectTransform с нужным размером и позицией для окна настроек
    /// </summary>
    private static RectTransform CreateRect(string name, Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        var gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);

        var rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;
        return rectTransform;
    }
    /// <summary>
    /// Создает картинку интерфейса внутри окна настроек
    /// </summary>
    private static Image CreateImage(string name, Transform parent, Sprite sprite)
    {
        var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        gameObject.transform.SetParent(parent, false);

        var image = gameObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = false;
        image.raycastTarget = false;
        return image;
    }
    /// <summary>
    /// Создает текстовую подпись или значение внутри окна настроек
    /// </summary>
    private static Text CreateText(string name, Transform parent, string value, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment, FontStyle fontStyle)
    {
        var rectTransform = CreateRect(name, parent, anchoredPosition, size);
        rectTransform.gameObject.AddComponent<CanvasRenderer>();

        var text = rectTransform.gameObject.AddComponent<Text>();
        text.text = value;
        text.font = GetDefaultFont();
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = Color.black;
        text.raycastTarget = false;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 14;
        text.resizeTextMaxSize = fontSize;
        return text;
    }
    /// <summary>
    /// Создает переключатель полного экрана в окне настроек
    /// </summary>
    private static Toggle CreateToggle(string name, Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        var background = CreateImage(name, parent, null);
        background.raycastTarget = true;
        background.color = new Color(0.16f, 0.24f, 0.28f, 0.96f);
        SetCentered(background.rectTransform, anchoredPosition, size);

        var checkmark = CreateImage("Checkmark", background.transform, null);
        checkmark.color = new Color(0.58f, 0.16f, 0.86f, 1f);
        SetCentered(checkmark.rectTransform, Vector2.zero, size * 0.58f);

        var toggle = background.gameObject.AddComponent<Toggle>();
        toggle.targetGraphic = background;
        toggle.graphic = checkmark;
        return toggle;
    }
    /// <summary>
    /// Создает ползунок громкости музыки в окне настроек
    /// </summary>
    private static Slider CreateSlider(string name, Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        var root = CreateRect(name, parent, anchoredPosition, size);

        var background = CreateImage("Background", root, null);
        background.color = new Color(0.2f, 0.2f, 0.22f, 1f);
        SetCentered(background.rectTransform, Vector2.zero, new Vector2(size.x, 10f));

        var fillArea = CreateRect("Fill Area", root, Vector2.zero, new Vector2(size.x - 26f, 10f));
        StretchHorizontally(fillArea);
        fillArea.offsetMin = new Vector2(13f, 12f);
        fillArea.offsetMax = new Vector2(-13f, -12f);

        var fill = CreateImage("Fill", fillArea, null);
        fill.color = new Color(0.14f, 0.54f, 0.62f, 1f);
        Stretch(fill.rectTransform);

        var handleArea = CreateRect("Handle Slide Area", root, Vector2.zero, new Vector2(size.x - 26f, size.y));
        StretchHorizontally(handleArea);
        handleArea.offsetMin = new Vector2(13f, 0f);
        handleArea.offsetMax = new Vector2(-13f, 0f);

        var handle = CreateImage("Handle", handleArea, null);
        handle.color = new Color(0.58f, 0.16f, 0.86f, 1f);
        SetCentered(handle.rectTransform, Vector2.zero, new Vector2(26f, 26f));
        handle.raycastTarget = true;

        var slider = root.gameObject.AddComponent<Slider>();
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.direction = Slider.Direction.LeftToRight;
        return slider;
    }
    /// <summary>
    /// Создает кнопку окна настроек с текстовой подписью
    /// </summary>
    private static Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPosition, Vector2 size)
    {
        var button = CreateButtonSurface(name, parent, new Color(0.58f, 0.16f, 0.86f, 1f), anchoredPosition, size);

        var text = CreateText("Label", button.transform, label, Vector2.zero, size, 26, TextAnchor.MiddleCenter, FontStyle.Bold);
        text.color = Color.white;
        return button;
    }
    /// <summary>
    /// Создает основу кнопки с цветом, размером и позицией
    /// </summary>
    private static Button CreateButtonSurface(string name, Transform parent, Color color, Vector2 anchoredPosition, Vector2 size)
    {
        var image = CreateImage(name, parent, null);
        image.raycastTarget = true;
        image.color = color;
        SetCentered(image.rectTransform, anchoredPosition, size);

        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        return button;
    }
    /// <summary>
    /// Выставляет RectTransform по центру с нужной позицией и размером
    /// </summary>
    private static void SetCentered(RectTransform rectTransform, Vector2 anchoredPosition, Vector2 size)
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;
    }
    /// <summary>
    /// Растягивает RectTransform на всю область родителя
    /// </summary>
    private static void Stretch(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
    /// <summary>
    /// Растягивает RectTransform по ширине родителя
    /// </summary>
    private static void StretchHorizontally(RectTransform rectTransform)
    {
        rectTransform.anchorMin = new Vector2(0f, 0.5f);
        rectTransform.anchorMax = new Vector2(1f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }
    /// <summary>
    /// Возвращает стандартный шрифт Unity для созданных из кода надписей
    /// </summary>
    private static Font GetDefaultFont()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
    }
}
