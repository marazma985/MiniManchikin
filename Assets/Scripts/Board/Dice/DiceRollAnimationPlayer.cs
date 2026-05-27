using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Проигрывает APNG-анимации кубика для разных игровых контекстов и сообщает, когда анимация закончилась
/// </summary>

public sealed class DiceRollAnimationPlayer : MonoBehaviour
{
    private const int MinDiceValue = 1;
    private const int MaxDiceValue = 6;
    private const float DefaultFrameDelay = 1f / 60f;

    [SerializeField] private string diceFolderRelativeToStreamingAssets = "Board/Dice";
    [SerializeField] private string diceFolderRelativeToAssets = "Art/Board/Dice";
    [SerializeField] private string diceFileNameFormat = "DiceRoll_{0}.png";
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
    private RawImage animationImage;
    private Coroutine currentRoutine;

    public static DiceRollAnimationPlayer Instance { get; private set; }

    public bool IsPlaying => currentRoutine != null;
    /// <summary>
    /// Выполняет вспомогательную часть логики метода TestBoardAnimation
    /// </summary>
    public void TestBoardAnimation()
    {
        PlayRandomPreview(DiceRollAnimationContext.Board);
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода TestBattleAnimation
    /// </summary>
    public void TestBattleAnimation()
    {
        PlayRandomPreview(DiceRollAnimationContext.Battle);
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода TestEscapeAnimation
    /// </summary>
    public void TestEscapeAnimation()
    {
        PlayRandomPreview(DiceRollAnimationContext.Escape);
    }
    /// <summary>
    /// Запускает музыку, анимацию или другой визуальный процесс
    /// </summary>
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
    /// <summary>
    /// Запускает музыку, анимацию или другой визуальный процесс
    /// </summary>
    public Coroutine Play(int result, DiceRollAnimationContext context = DiceRollAnimationContext.Board, Action onCompleted = null)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(PlayAndNotify(result, context, onCompleted));
        return currentRoutine;
    }
    /// <summary>
    /// Запускает музыку, анимацию или другой визуальный процесс
    /// </summary>
    public IEnumerator PlayRoutine(int result, DiceRollAnimationContext context = DiceRollAnimationContext.Board)
    {
        EnsureOverlay();
        ApplySettings(GetSettings(context));

        var diceValue = Mathf.Clamp(result, MinDiceValue, MaxDiceValue);
        var filePath = GetDiceFilePath(diceValue);
        overlayCanvas.enabled = true;
        animationImage.enabled = true;

        if (File.Exists(filePath))
        {
            var bytes = File.ReadAllBytes(filePath);
            yield return PlayApng(bytes);
        }
        else
        {
            Debug.LogWarning($"Dice animation file not found: {filePath}");
            animationImage.texture = null;
            animationImage.enabled = false;
            yield return new WaitForSecondsRealtime(fallbackDisplaySeconds);
        }

        animationImage.texture = null;
        animationImage.enabled = false;
        overlayCanvas.enabled = false;
    }
    /// <summary>
    /// Запускает музыку, анимацию или другой визуальный процесс
    /// </summary>
    private IEnumerator PlayAndNotify(int result, DiceRollAnimationContext context, Action onCompleted)
    {
        yield return PlayRoutine(result, context);
        currentRoutine = null;
        onCompleted?.Invoke();
    }
    /// <summary>
    /// Инициализирует ссылки и внутреннее состояние до запуска сцены
    /// </summary>
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
    /// <summary>
    /// Освобождает подписки и временные ресурсы перед уничтожением объекта
    /// </summary>
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
    /// <summary>
    /// Гарантирует, что нужный объект, ресурс или ссылка существует
    /// </summary>
    private void EnsureOverlay()
    {
        if (overlayCanvas != null && animationImage != null)
            return;

        // Оверлей создается по требованию, чтобы сцены могли настраивать компонент без ручного создания дочерних объектов
        overlayCanvas = GetComponentInChildren<Canvas>(true);
        if (overlayCanvas == null)
        {
            var canvasObject = new GameObject("Dice Roll Overlay");
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

        animationImage = overlayCanvas.GetComponentInChildren<RawImage>(true);
        if (animationImage == null)
        {
            var imageObject = new GameObject("Dice Roll Image");
            imageObject.transform.SetParent(overlayCanvas.transform, false);
            animationImage = imageObject.AddComponent<RawImage>();
        }

        animationImage.raycastTarget = false;
        animationImage.enabled = false;

        var rectTransform = animationImage.rectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        ApplySettings(boardAnimation);
    }
    /// <summary>
    /// Возвращает сохраненное или рассчитанное значение
    /// </summary>
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
    /// <summary>
    /// Запускает музыку, анимацию или другой визуальный процесс
    /// </summary>
    private void PlayRandomPreview(DiceRollAnimationContext context)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Dice animation previews are available in Play Mode.");
            return;
        }

        Play(UnityEngine.Random.Range(MinDiceValue, MaxDiceValue + 1), context);
    }
    /// <summary>
    /// Применяет изменение к игровому или визуальному состоянию
    /// </summary>
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
    /// <summary>
    /// Возвращает сохраненное или рассчитанное значение
    /// </summary>
    private string GetDiceFilePath(int result)
    {
        var fileName = string.Format(diceFileNameFormat, result);
        var streamingPath = Path.Combine(Application.streamingAssetsPath, diceFolderRelativeToStreamingAssets, fileName);
        if (File.Exists(streamingPath))
            return streamingPath;

        return Path.Combine(Application.dataPath, diceFolderRelativeToAssets, fileName);
    }
    /// <summary>
    /// Запускает музыку, анимацию или другой визуальный процесс
    /// </summary>
    private IEnumerator PlayApng(byte[] bytes)
    {
        if (!ApngFile.TryParse(bytes, out var apngFile) || apngFile.Frames.Count == 0)
        {
            yield return PlayFallbackPng(bytes);
            yield break;
        }

        // Кадры APNG собираются на одном холсте, потому что многие кадры содержат только изменившиеся пиксели
        var canvasPixels = CreateTransparentPixels(apngFile.Width, apngFile.Height);
        var outputTexture = new Texture2D(apngFile.Width, apngFile.Height, TextureFormat.RGBA32, false);
        outputTexture.wrapMode = TextureWrapMode.Clamp;
        outputTexture.filterMode = FilterMode.Bilinear;
        animationImage.texture = outputTexture;

        for (var i = 0; i < apngFile.Frames.Count; i++)
        {
            var frame = apngFile.Frames[i];
            var framePng = apngFile.BuildFramePng(frame);
            var frameTexture = new Texture2D(frame.Width, frame.Height, TextureFormat.RGBA32, false);
            if (!frameTexture.LoadImage(framePng, false))
            {
                Destroy(frameTexture);
                continue;
            }

            // Для DisposeOp.Previous нужна копия холста до отрисовки текущего кадра
            var previousPixels = frame.DisposeOp == ApngDisposeOp.Previous ? ClonePixels(canvasPixels) : null;
            BlendFrame(canvasPixels, apngFile.Width, apngFile.Height, frame, frameTexture.GetPixels32());
            outputTexture.SetPixels32(canvasPixels);
            outputTexture.Apply(false);

            Destroy(frameTexture);

            var delay = frame.DelaySeconds > 0f ? frame.DelaySeconds : DefaultFrameDelay;
            yield return new WaitForSecondsRealtime(delay);

            ApplyDispose(canvasPixels, apngFile.Width, apngFile.Height, frame, previousPixels);
        }

        Destroy(outputTexture);
    }
    /// <summary>
    /// Запускает музыку, анимацию или другой визуальный процесс
    /// </summary>
    private IEnumerator PlayFallbackPng(byte[] bytes)
    {
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (texture.LoadImage(bytes, false))
        {
            animationImage.texture = texture;
            yield return new WaitForSecondsRealtime(fallbackDisplaySeconds);
        }
        else
        {
            Debug.LogWarning("Dice animation PNG could not be loaded.");
            animationImage.texture = null;
            animationImage.enabled = false;
            yield return new WaitForSecondsRealtime(fallbackDisplaySeconds);
        }

        Destroy(texture);
    }
    /// <summary>
    /// Создает объект или набор данных, который дальше использует система
    /// </summary>
    private static Color32[] CreateTransparentPixels(int width, int height)
    {
        return new Color32[width * height];
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода ClonePixels
    /// </summary>
    private static Color32[] ClonePixels(Color32[] pixels)
    {
        var clone = new Color32[pixels.Length];
        Array.Copy(pixels, clone, pixels.Length);
        return clone;
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода BlendFrame
    /// </summary>
    private static void BlendFrame(Color32[] canvasPixels, int canvasWidth, int canvasHeight, ApngFrame frame, Color32[] framePixels)
    {
        var canvasBottomY = canvasHeight - frame.YOffset - frame.Height;
        for (var y = 0; y < frame.Height; y++)
        {
            var canvasY = canvasBottomY + y;
            if (canvasY < 0 || canvasY >= canvasHeight)
                continue;

            for (var x = 0; x < frame.Width; x++)
            {
                var canvasX = frame.XOffset + x;
                if (canvasX < 0 || canvasX >= canvasWidth)
                    continue;

                var source = framePixels[y * frame.Width + x];
                var targetIndex = canvasY * canvasWidth + canvasX;
                canvasPixels[targetIndex] = frame.BlendOp == ApngBlendOp.Source
                    ? source
                    : AlphaBlend(source, canvasPixels[targetIndex]);
            }
        }
    }
    /// <summary>
    /// Выполняет вспомогательную часть логики метода AlphaBlend
    /// </summary>
    private static Color32 AlphaBlend(Color32 source, Color32 destination)
    {
        if (source.a == 255)
            return source;

        if (source.a == 0)
            return destination;

        var sourceAlpha = source.a / 255f;
        var destinationAlpha = destination.a / 255f;
        var outputAlpha = sourceAlpha + destinationAlpha * (1f - sourceAlpha);
        if (outputAlpha <= 0f)
            return new Color32(0, 0, 0, 0);

        var red = (source.r * sourceAlpha + destination.r * destinationAlpha * (1f - sourceAlpha)) / outputAlpha;
        var green = (source.g * sourceAlpha + destination.g * destinationAlpha * (1f - sourceAlpha)) / outputAlpha;
        var blue = (source.b * sourceAlpha + destination.b * destinationAlpha * (1f - sourceAlpha)) / outputAlpha;

        return new Color32(
            (byte)Mathf.RoundToInt(red),
            (byte)Mathf.RoundToInt(green),
            (byte)Mathf.RoundToInt(blue),
            (byte)Mathf.RoundToInt(outputAlpha * 255f));
    }
    /// <summary>
    /// Применяет изменение к игровому или визуальному состоянию
    /// </summary>
    private static void ApplyDispose(Color32[] canvasPixels, int canvasWidth, int canvasHeight, ApngFrame frame, Color32[] previousPixels)
    {
        switch (frame.DisposeOp)
        {
            case ApngDisposeOp.Background:
                ClearFrameArea(canvasPixels, canvasWidth, canvasHeight, frame);
                break;
            case ApngDisposeOp.Previous:
                if (previousPixels != null)
                    Array.Copy(previousPixels, canvasPixels, canvasPixels.Length);
                break;
        }
    }
    /// <summary>
    /// Очищает текущее состояние и возвращает систему к пустому виду
    /// </summary>
    private static void ClearFrameArea(Color32[] canvasPixels, int canvasWidth, int canvasHeight, ApngFrame frame)
    {
        var canvasBottomY = canvasHeight - frame.YOffset - frame.Height;
        for (var y = 0; y < frame.Height; y++)
        {
            var canvasY = canvasBottomY + y;
            if (canvasY < 0 || canvasY >= canvasHeight)
                continue;

            for (var x = 0; x < frame.Width; x++)
            {
                var canvasX = frame.XOffset + x;
                if (canvasX < 0 || canvasX >= canvasWidth)
                    continue;

                canvasPixels[canvasY * canvasWidth + canvasX] = new Color32(0, 0, 0, 0);
            }
        }
    }
    /// <summary>
    /// Перечисляет варианты apng dispose op, которые используются в игровой логике вместо строковых значений
    /// </summary>
    private enum ApngDisposeOp
    {
        None = 0,
        Background = 1,
        Previous = 2
    }
    /// <summary>
    /// Перечисляет варианты apng blend op, которые используются в игровой логике вместо строковых значений
    /// </summary>
    private enum ApngBlendOp
    {
        Source = 0,
        Over = 1
    }
    /// <summary>
    /// Отвечает за механику или визуал кубика, связанные с ApngFrame
    /// </summary>
    private sealed class ApngFrame
    {
        public int Width;
        public int Height;
        public int XOffset;
        public int YOffset;
        public float DelaySeconds;
        public ApngDisposeOp DisposeOp;
        public ApngBlendOp BlendOp;
        public readonly List<byte[]> ImageParts = new List<byte[]>();
    }
    /// <summary>
    /// Отвечает за механику или визуал кубика, связанные с PngChunk
    /// </summary>
    private sealed class PngChunk
    {
        public string Type;
        public byte[] Data;
        /// <summary>
        /// Создает экземпляр PngChunk и заполняет его начальными данными
        /// </summary>
        public PngChunk(string type, byte[] data)
        {
            Type = type;
            Data = data;
        }
    }
    /// <summary>
    /// Отвечает за механику или визуал кубика, связанные с ApngFile
    /// </summary>
    private sealed class ApngFile
    {
        private static readonly byte[] PngSignature = { 137, 80, 78, 71, 13, 10, 26, 10 };

        private readonly byte[] ihdrData;
        private readonly List<PngChunk> headerChunks;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public List<ApngFrame> Frames { get; } = new List<ApngFrame>();
        /// <summary>
        /// Создает экземпляр ApngFile и заполняет его начальными данными
        /// </summary>
        private ApngFile(byte[] ihdrData, List<PngChunk> headerChunks)
        {
            this.ihdrData = ihdrData;
            this.headerChunks = headerChunks;
            Width = ReadInt32BigEndian(ihdrData, 0);
            Height = ReadInt32BigEndian(ihdrData, 4);
        }
        /// <summary>
        /// Пытается выполнить действие и возвращает, получилось ли это сделать
        /// </summary>
        public static bool TryParse(byte[] bytes, out ApngFile apngFile)
        {
            apngFile = null;
            if (bytes == null || bytes.Length < PngSignature.Length + 12 || !HasPngSignature(bytes))
                return false;

            var offset = PngSignature.Length;
            byte[] ihdr = null;
            var headerChunks = new List<PngChunk>();
            var frames = new List<ApngFrame>();
            ApngFrame currentFrame = null;
            var hasAnimationControl = false;
            var sawImageData = false;

            while (offset + 12 <= bytes.Length)
            {
                var length = ReadInt32BigEndian(bytes, offset);
                var type = ReadAscii(bytes, offset + 4, 4);
                var dataOffset = offset + 8;
                if (length < 0 || dataOffset + length + 4 > bytes.Length)
                    return false;

                var data = CopyBytes(bytes, dataOffset, length);

                switch (type)
                {
                    case "IHDR":
                        ihdr = data;
                        break;
                    case "acTL":
                        hasAnimationControl = true;
                        break;
                    case "fcTL":
                        currentFrame = ParseFrameControl(data);
                        frames.Add(currentFrame);
                        break;
                    case "IDAT":
                        sawImageData = true;
                        if (currentFrame != null)
                            currentFrame.ImageParts.Add(data);
                        break;
                    case "fdAT":
                        sawImageData = true;
                        if (currentFrame != null && data.Length > 4)
                            currentFrame.ImageParts.Add(CopyBytes(data, 4, data.Length - 4));
                        break;
                    case "IEND":
                        offset = bytes.Length;
                        continue;
                    default:
                        if (!sawImageData && type != "acTL")
                            headerChunks.Add(new PngChunk(type, data));
                        break;
                }

                offset += length + 12;
            }

            if (!hasAnimationControl || ihdr == null || frames.Count == 0)
                return false;

            apngFile = new ApngFile(ihdr, headerChunks);
            apngFile.Frames.AddRange(frames);
            return true;
        }
        /// <summary>
        /// Собирает набор данных из отдельных частей
        /// </summary>
        public byte[] BuildFramePng(ApngFrame frame)
        {
            var chunks = new List<byte>();
            chunks.AddRange(PngSignature);

            var frameIhdr = CopyBytes(ihdrData, 0, ihdrData.Length);
            WriteInt32BigEndian(frameIhdr, 0, frame.Width);
            WriteInt32BigEndian(frameIhdr, 4, frame.Height);
            WriteChunk(chunks, "IHDR", frameIhdr);

            for (var i = 0; i < headerChunks.Count; i++)
                WriteChunk(chunks, headerChunks[i].Type, headerChunks[i].Data);

            for (var i = 0; i < frame.ImageParts.Count; i++)
                WriteChunk(chunks, "IDAT", frame.ImageParts[i]);

            WriteChunk(chunks, "IEND", new byte[0]);
            return chunks.ToArray();
        }
        /// <summary>
        /// Выполняет вспомогательную часть логики метода ParseFrameControl
        /// </summary>
        private static ApngFrame ParseFrameControl(byte[] data)
        {
            var delayNumerator = ReadUInt16BigEndian(data, 20);
            var delayDenominator = ReadUInt16BigEndian(data, 22);
            if (delayDenominator == 0)
                delayDenominator = 100;

            return new ApngFrame
            {
                Width = ReadInt32BigEndian(data, 4),
                Height = ReadInt32BigEndian(data, 8),
                XOffset = ReadInt32BigEndian(data, 12),
                YOffset = ReadInt32BigEndian(data, 16),
                DelaySeconds = delayNumerator > 0 ? delayNumerator / (float)delayDenominator : DefaultFrameDelay,
                DisposeOp = (ApngDisposeOp)data[24],
                BlendOp = (ApngBlendOp)data[25]
            };
        }
        /// <summary>
        /// Проверяет, есть ли нужное состояние или данные
        /// </summary>
        private static bool HasPngSignature(byte[] bytes)
        {
            for (var i = 0; i < PngSignature.Length; i++)
            {
                if (bytes[i] != PngSignature[i])
                    return false;
            }

            return true;
        }
        /// <summary>
        /// Выполняет вспомогательную часть логики метода WriteChunk
        /// </summary>
        private static void WriteChunk(List<byte> output, string type, byte[] data)
        {
            var typeBytes = System.Text.Encoding.ASCII.GetBytes(type);
            var lengthBytes = new byte[4];
            WriteInt32BigEndian(lengthBytes, 0, data.Length);
            output.AddRange(lengthBytes);
            output.AddRange(typeBytes);
            output.AddRange(data);

            var crcInput = new byte[typeBytes.Length + data.Length];
            Array.Copy(typeBytes, 0, crcInput, 0, typeBytes.Length);
            Array.Copy(data, 0, crcInput, typeBytes.Length, data.Length);

            var crcBytes = new byte[4];
            WriteUInt32BigEndian(crcBytes, 0, Crc32.Compute(crcInput));
            output.AddRange(crcBytes);
        }
        /// <summary>
        /// Выполняет вспомогательную часть логики метода CopyBytes
        /// </summary>
        private static byte[] CopyBytes(byte[] source, int offset, int length)
        {
            var copy = new byte[length];
            Array.Copy(source, offset, copy, 0, length);
            return copy;
        }
        /// <summary>
        /// Выполняет вспомогательную часть логики метода ReadAscii
        /// </summary>
        private static string ReadAscii(byte[] bytes, int offset, int count)
        {
            return System.Text.Encoding.ASCII.GetString(bytes, offset, count);
        }
        /// <summary>
        /// Выполняет вспомогательную часть логики метода ReadInt32BigEndian
        /// </summary>
        private static int ReadInt32BigEndian(byte[] bytes, int offset)
        {
            return (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
        }
        /// <summary>
        /// Выполняет вспомогательную часть логики метода ReadUInt16BigEndian
        /// </summary>
        private static ushort ReadUInt16BigEndian(byte[] bytes, int offset)
        {
            return (ushort)((bytes[offset] << 8) | bytes[offset + 1]);
        }
        /// <summary>
        /// Выполняет вспомогательную часть логики метода WriteInt32BigEndian
        /// </summary>
        private static void WriteInt32BigEndian(byte[] bytes, int offset, int value)
        {
            bytes[offset] = (byte)((value >> 24) & 0xff);
            bytes[offset + 1] = (byte)((value >> 16) & 0xff);
            bytes[offset + 2] = (byte)((value >> 8) & 0xff);
            bytes[offset + 3] = (byte)(value & 0xff);
        }
        /// <summary>
        /// Выполняет вспомогательную часть логики метода WriteUInt32BigEndian
        /// </summary>
        private static void WriteUInt32BigEndian(byte[] bytes, int offset, uint value)
        {
            bytes[offset] = (byte)((value >> 24) & 0xff);
            bytes[offset + 1] = (byte)((value >> 16) & 0xff);
            bytes[offset + 2] = (byte)((value >> 8) & 0xff);
            bytes[offset + 3] = (byte)(value & 0xff);
        }
    }
    /// <summary>
    /// Отвечает за механику или визуал кубика, связанные с Crc32
    /// </summary>
    private static class Crc32
    {
        private const uint Polynomial = 0xedb88320u;
        private static readonly uint[] Table = BuildTable();
        /// <summary>
        /// Выполняет вспомогательную часть логики метода Compute
        /// </summary>
        public static uint Compute(byte[] bytes)
        {
            var crc = 0xffffffffu;
            for (var i = 0; i < bytes.Length; i++)
                crc = Table[(int)((crc ^ bytes[i]) & 0xff)] ^ (crc >> 8);

            return crc ^ 0xffffffffu;
        }
        /// <summary>
        /// Собирает набор данных из отдельных частей
        /// </summary>
        private static uint[] BuildTable()
        {
            var table = new uint[256];
            for (uint i = 0; i < table.Length; i++)
            {
                var value = i;
                for (var bit = 0; bit < 8; bit++)
                    value = (value & 1) != 0 ? Polynomial ^ (value >> 1) : value >> 1;

                table[i] = value;
            }

            return table;
        }
    }
}
/// <summary>
/// Перечисляет варианты dice roll animation context, которые используются в игровой логике вместо строковых значений
/// </summary>
public enum DiceRollAnimationContext
{
    Board,
    Battle,
    Escape
}
/// <summary>
/// Отвечает за механику или визуал кубика, связанные с DiceRollAnimationSettings
/// </summary>
[Serializable]
public sealed class DiceRollAnimationSettings
{
    public Vector2 Size = new Vector2(420f, 420f);
    public Vector2 AnchoredPosition = Vector2.zero;
    public int SortingOrder = 32760;
}
