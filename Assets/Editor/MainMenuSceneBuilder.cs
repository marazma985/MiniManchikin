using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MainMenuSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/MainMenu.unity";
    private const string BackgroundPath = "Assets/Art/MainMenuArt/background.png";
    private const string ButtonsPath = "Assets/Art/MainMenuArt/buttons.png";
    private const string ModalSquarePath = "Assets/Art/ShareArt/ModalSquare.png";
    private const string BlurMaterialPath = "Assets/Materials/UIBackgroundBlur.mat";
    private const string MainAudioMixerPath = "Assets/Audio/MainAudioMixer.mixer";
    private static readonly Vector2 ReferenceResolution = new Vector2(1672f, 941f);

    [MenuItem("Tools/Board Mili/Create Main Menu Scene")]
    public static void CreateMainMenuScene()
    {
        var background = LoadSprite(BackgroundPath, "background_0", "background");
        var logo = LoadSprite(ButtonsPath, "buttons_0", "button_0");
        var continueLocked = LoadSprite(ButtonsPath, "buttons_1", "button_1");
        var continueAvailable = LoadSprite(ButtonsPath, "buttons_2", "button_2");
        var newGame = LoadSprite(ButtonsPath, "buttons_3", "button_3");
        var settings = LoadSprite(ButtonsPath, "buttons_11", "button_11");
        var exit = LoadSprite(ButtonsPath, "buttons_15", "button_15");
        var modalSquare = LoadSprite(ModalSquarePath, "ModalSquare_0", "ModalSquare");
        var blurMaterial = AssetDatabase.LoadAssetAtPath<Material>(BlurMaterialPath);
        var mainAudioMixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MainAudioMixerPath);
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MainMenu";

        var camera = CreateCamera();
        CreateEventSystem();

        var canvas = CreateCanvas(camera);
        var root = CreateRect("Root", canvas.transform);
        Stretch(root);

        var backgroundImage = CreateImage("Background", root, background, true);
        Stretch(backgroundImage.rectTransform);
        backgroundImage.preserveAspect = false;

        var logoImage = CreateImage("Game Title", root, logo, true);
        SetCentered(logoImage.rectTransform, new Vector2(0f, 270f), SpriteSize(logo, 0.98f));

        var buttonsRoot = CreateRect("Buttons", root);
        SetCentered(buttonsRoot, new Vector2(0f, -90f), new Vector2(460f, 500f));

        CreateMenuButton(buttonsRoot, "Continue Button", MainMenuSpriteButton.VisualKind.Continue, new Vector2(0f, 165f), continueAvailable, continueLocked, newGame, settings, exit, false);
        CreateMenuButton(buttonsRoot, "New Game Button", MainMenuSpriteButton.VisualKind.NewGame, new Vector2(0f, 55f), continueAvailable, continueLocked, newGame, settings, exit, true);
        var settingsButton = CreateMenuButton(buttonsRoot, "Settings Button", MainMenuSpriteButton.VisualKind.Settings, new Vector2(0f, -55f), continueAvailable, continueLocked, newGame, settings, exit, true);
        CreateMenuButton(buttonsRoot, "Exit Button", MainMenuSpriteButton.VisualKind.Exit, new Vector2(0f, -165f), continueAvailable, continueLocked, newGame, settings, exit, true);
        CreateSettingsSystem(canvas, camera, settingsButton, modalSquare, blurMaterial, mainAudioMixer);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        Debug.Log($"Main menu scene created at {ScenePath}");
    }

    private static Camera CreateCamera()
    {
        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        return camera;
    }

    private static void CreateEventSystem()
    {
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    private static Canvas CreateCanvas(Camera camera)
    {
        var canvasObject = new GameObject("Main Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 10f;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = ReferenceResolution;
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private static Button CreateMenuButton(RectTransform parent, string name, MainMenuSpriteButton.VisualKind kind, Vector2 anchoredPosition, Sprite continueAvailable, Sprite continueLocked, Sprite newGame, Sprite settings, Sprite exit, bool interactable)
    {
        var image = CreateImage(name, parent, null, true);
        SetCentered(image.rectTransform, anchoredPosition, SpriteSize(kind == MainMenuSpriteButton.VisualKind.Continue ? continueLocked : SpriteFor(kind, newGame, settings, exit), 1f));

        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        button.interactable = interactable;
        image.color = Color.white;


        var feedback = image.gameObject.AddComponent<MainMenuButtonFeedback>();
        var feedbackSerialized = new SerializedObject(feedback);
        feedbackSerialized.FindProperty("target").objectReferenceValue = image.rectTransform;
        feedbackSerialized.FindProperty("hoverLift").floatValue = 5f;
        feedbackSerialized.FindProperty("moveSmoothTime").floatValue = 0.055f;
        feedbackSerialized.ApplyModifiedPropertiesWithoutUndo();

        var skin = image.gameObject.AddComponent<MainMenuSpriteButton>();
        var serialized = new SerializedObject(skin);
        serialized.FindProperty("kind").enumValueIndex = (int)kind;
        serialized.FindProperty("continueAvailable").boolValue = interactable;
        serialized.FindProperty("targetImage").objectReferenceValue = image;
        serialized.FindProperty("button").objectReferenceValue = button;
        serialized.FindProperty("continueAvailableSprite").objectReferenceValue = continueAvailable;
        serialized.FindProperty("continueLockedSprite").objectReferenceValue = continueLocked;
        serialized.FindProperty("newGameSprite").objectReferenceValue = newGame;
        serialized.FindProperty("settingsSprite").objectReferenceValue = settings;
        serialized.FindProperty("exitSprite").objectReferenceValue = exit;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        skin.ApplyVisual();

        image.gameObject.AddComponent<MainMenuCursorHoverTarget>();
        return button;
    }

    private static void CreateSettingsSystem(Canvas canvas, Camera camera, Button settingsButton, Sprite modalSquare, Material blurMaterial, AudioMixer mainAudioMixer)
    {
        var settingsService = canvas.gameObject.AddComponent<GameSettingsService>();
        var settingsServiceSerialized = new SerializedObject(settingsService);
        settingsServiceSerialized.FindProperty("mainAudioMixer").objectReferenceValue = mainAudioMixer;
        settingsServiceSerialized.ApplyModifiedPropertiesWithoutUndo();

        var modalView = canvas.gameObject.AddComponent<MainMenuSettingsModalView>();
        var modalSerialized = new SerializedObject(modalView);
        modalSerialized.FindProperty("canvas").objectReferenceValue = canvas;
        modalSerialized.FindProperty("sourceCamera").objectReferenceValue = camera;
        modalSerialized.FindProperty("modalSprite").objectReferenceValue = modalSquare;
        modalSerialized.FindProperty("blurMaterial").objectReferenceValue = blurMaterial;
        modalSerialized.FindProperty("settingsService").objectReferenceValue = settingsService;
        modalSerialized.FindProperty("openButton").objectReferenceValue = settingsButton;
        modalSerialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Sprite SpriteFor(MainMenuSpriteButton.VisualKind kind, Sprite newGame, Sprite settings, Sprite exit)
    {
        switch (kind)
        {
            case MainMenuSpriteButton.VisualKind.NewGame:
                return newGame;
            case MainMenuSpriteButton.VisualKind.Settings:
                return settings;
            case MainMenuSpriteButton.VisualKind.Exit:
                return exit;
            default:
                return newGame;
        }
    }

    private static Image CreateImage(string name, Transform parent, Sprite sprite, bool preserveAspect)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = preserveAspect;
        image.raycastTarget = true;
        return image;
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void SetCentered(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private static Vector2 SpriteSize(Sprite sprite, float scale)
    {
        if (sprite == null)
            return Vector2.zero;

        return sprite.rect.size * scale;
    }

    private static Sprite LoadSprite(string assetPath, params string[] names)
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Sprite>().ToArray();
        foreach (var name in names)
        {
            var sprite = sprites.FirstOrDefault(candidate => string.Equals(candidate.name, name, StringComparison.OrdinalIgnoreCase));
            if (sprite != null)
                return sprite;
        }

        throw new InvalidOperationException($"Could not find sprite '{string.Join("' or '", names)}' in {assetPath}.");
    }
}
