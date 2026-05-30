using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.UI;

public sealed class DiceRollAnimationPlayer : MonoBehaviour
{
    private const int MinDiceValue = 1;
    private const int MaxDiceValue = 6;

    [SerializeField] private string clipsResourcePath = "UI/DiceRollAnimations";
    [SerializeField] private AnimationClip[] rollClips = new AnimationClip[MaxDiceValue];
    [SerializeField] private DiceRollAnimationSettings boardAnimation = new DiceRollAnimationSettings
    {
        Size = new Vector2(420f, 420f),
        AnchoredPosition = Vector2.zero,
        SortingOrder = 32760
    };
    [SerializeField] private DiceRollAnimationSettings battleAnimation = new DiceRollAnimationSettings
    {
        Size = new Vector2(360f, 360f),
        AnchoredPosition = Vector2.zero,
        SortingOrder = 32760
    };
    [SerializeField] private DiceRollAnimationSettings escapeAnimation = new DiceRollAnimationSettings
    {
        Size = new Vector2(360f, 360f),
        AnchoredPosition = Vector2.zero,
        SortingOrder = 32760
    };
    [SerializeField, Min(0.1f)] private float fallbackDisplaySeconds = 1.2f;

    private Canvas overlayCanvas;
    private Image animationImage;
    private Animator animationAnimator;
    private PlayableGraph playableGraph;
    private Coroutine currentRoutine;

    public static DiceRollAnimationPlayer Instance { get; private set; }

    public bool IsPlaying => currentRoutine != null;

    public void TestBoardAnimation()
    {
        PlayRandomPreview(DiceRollAnimationContext.Board);
    }

    public void TestBattleAnimation()
    {
        PlayRandomPreview(DiceRollAnimationContext.Battle);
    }

    public void TestEscapeAnimation()
    {
        PlayRandomPreview(DiceRollAnimationContext.Escape);
    }

    public static IEnumerator PlayGlobalRoutine(int result, DiceRollAnimationContext context = DiceRollAnimationContext.Board)
    {
        var player = Instance != null ? Instance : FindAnyObjectByType<DiceRollAnimationPlayer>();
        if (player == null)
        {
            var playerObject = new GameObject("Dice Roll Animation Player");
            player = playerObject.AddComponent<DiceRollAnimationPlayer>();
        }

        yield return player.PlayRoutine(result, context);
    }

    public Coroutine Play(int result, DiceRollAnimationContext context = DiceRollAnimationContext.Board, Action onCompleted = null)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            StopAnimationPlayback();
            HideOverlay();
        }

        currentRoutine = StartCoroutine(PlayAndNotify(result, context, onCompleted));
        return currentRoutine;
    }

    public IEnumerator PlayRoutine(int result, DiceRollAnimationContext context = DiceRollAnimationContext.Board)
    {
        EnsureOverlay();
        ApplySettings(GetSettings(context));

        var diceValue = Mathf.Clamp(result, MinDiceValue, MaxDiceValue);
        var clip = GetRollClip(diceValue);

        ShowOverlay();

        if (clip == null)
        {
            Debug.LogWarning($"Dice roll animation clip is missing for result {diceValue}.");
            yield return new WaitForSecondsRealtime(fallbackDisplaySeconds);
            HideOverlay();
            yield break;
        }

        PlayClip(clip);
        yield return new WaitForSecondsRealtime(Mathf.Max(clip.length, fallbackDisplaySeconds));
        StopAnimationPlayback();
        HideOverlay();
    }

    private IEnumerator PlayAndNotify(int result, DiceRollAnimationContext context, Action onCompleted)
    {
        yield return PlayRoutine(result, context);
        currentRoutine = null;
        onCompleted?.Invoke();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureOverlay();
    }

    private void OnDestroy()
    {
        StopAnimationPlayback();

        if (Instance == this)
            Instance = null;
    }

    private void OnDisable()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        StopAnimationPlayback();
        HideOverlay();
    }

    private void EnsureOverlay()
    {
        if (overlayCanvas != null && animationImage != null && animationAnimator != null)
            return;

        overlayCanvas = GetComponentInChildren<Canvas>(true);
        if (overlayCanvas == null)
        {
            var canvasObject = new GameObject("Dice Roll Overlay", typeof(RectTransform));
            canvasObject.transform.SetParent(transform, false);
            overlayCanvas = canvasObject.AddComponent<Canvas>();
        }

        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.enabled = false;

        var scaler = overlayCanvas.GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = overlayCanvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        if (overlayCanvas.GetComponent<GraphicRaycaster>() == null)
            overlayCanvas.gameObject.AddComponent<GraphicRaycaster>();

        animationImage = overlayCanvas.GetComponentInChildren<Image>(true);
        if (animationImage == null)
        {
            var imageObject = new GameObject("Dice Roll Image", typeof(RectTransform), typeof(CanvasRenderer));
            imageObject.transform.SetParent(overlayCanvas.transform, false);
            animationImage = imageObject.AddComponent<Image>();
        }

        animationImage.raycastTarget = false;
        animationImage.enabled = false;
        animationImage.preserveAspect = true;

        animationAnimator = animationImage.GetComponent<Animator>();
        if (animationAnimator == null)
            animationAnimator = animationImage.gameObject.AddComponent<Animator>();
        animationAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        animationAnimator.enabled = false;

        var rectTransform = animationImage.rectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        ApplySettings(boardAnimation);
    }

    private DiceRollAnimationSettings GetSettings(DiceRollAnimationContext context)
    {
        switch (context)
        {
            case DiceRollAnimationContext.Battle:
                return battleAnimation;
            case DiceRollAnimationContext.Escape:
                return escapeAnimation;
            default:
                return boardAnimation;
        }
    }

    private void PlayRandomPreview(DiceRollAnimationContext context)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Dice animation previews are available in Play Mode.");
            return;
        }

        Play(UnityEngine.Random.Range(MinDiceValue, MaxDiceValue + 1), context);
    }

    private void ApplySettings(DiceRollAnimationSettings settings)
    {
        if (settings == null)
            return;

        if (overlayCanvas != null)
            overlayCanvas.sortingOrder = settings.SortingOrder;

        if (animationImage == null)
            return;

        var rectTransform = animationImage.rectTransform;
        rectTransform.anchoredPosition = settings.AnchoredPosition;
        rectTransform.sizeDelta = settings.Size;
    }

    private AnimationClip GetRollClip(int diceValue)
    {
        var index = diceValue - 1;
        if (rollClips != null && index >= 0 && index < rollClips.Length && rollClips[index] != null)
            return rollClips[index];

        return Resources.Load<AnimationClip>($"{clipsResourcePath}/DiceRoll_{diceValue}");
    }

    private void ShowOverlay()
    {
        overlayCanvas.enabled = true;
        animationImage.enabled = true;
        animationAnimator.enabled = true;
    }

    private void HideOverlay()
    {
        if (animationImage != null)
        {
            animationImage.sprite = null;
            animationImage.enabled = false;
        }

        if (animationAnimator != null)
            animationAnimator.enabled = false;

        if (overlayCanvas != null)
            overlayCanvas.enabled = false;
    }

    private void PlayClip(AnimationClip clip)
    {
        StopAnimationPlayback();

        playableGraph = PlayableGraph.Create("Dice Roll Animation");
        playableGraph.SetTimeUpdateMode(DirectorUpdateMode.UnscaledGameTime);

        var output = AnimationPlayableOutput.Create(playableGraph, "Dice Roll Output", animationAnimator);
        var playable = AnimationClipPlayable.Create(playableGraph, clip);
        playable.SetApplyFootIK(false);
        output.SetSourcePlayable(playable);

        playableGraph.Play();
    }

    private void StopAnimationPlayback()
    {
        if (playableGraph.IsValid())
            playableGraph.Destroy();
    }
}

public enum DiceRollAnimationContext
{
    Board,
    Battle,
    Escape
}

[Serializable]
public sealed class DiceRollAnimationSettings
{
    public Vector2 Size = new Vector2(420f, 420f);
    public Vector2 AnchoredPosition = Vector2.zero;
    public int SortingOrder = 32760;
}
