using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class DiceRollAnimationAssetBuilder
{
    private const int MinDiceValue = 1;
    private const int MaxDiceValue = 6;
    private const float FrameRate = 60f;
    private const string FrameRoot = "Assets/Art/Board/Dice/Frames";
    private const string ClipRoot = "Assets/Resources/UI/DiceRollAnimations";

    [InitializeOnLoadMethod]
    private static void BuildMissingClipsAfterImport()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (!AssetDatabase.IsValidFolder(FrameRoot))
                return;

            var hasMissingClip = Enumerable.Range(MinDiceValue, MaxDiceValue)
                .Any(value => AssetDatabase.LoadAssetAtPath<AnimationClip>(GetClipPath(value)) == null);

            if (hasMissingClip)
                Rebuild();
        };
    }

    [MenuItem("Tools/Board/Dice/Rebuild Roll Animations")]
    public static void Rebuild()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/UI");
        EnsureFolder(ClipRoot);

        for (var value = MinDiceValue; value <= MaxDiceValue; value++)
            ConfigureFrameImporters(value);

        AssetDatabase.Refresh();

        for (var value = MinDiceValue; value <= MaxDiceValue; value++)
            BuildClip(value);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void ConfigureFrameImporters(int value)
    {
        foreach (var framePath in GetFramePaths(value))
        {
            var importer = AssetImporter.GetAtPath(framePath) as TextureImporter;
            if (importer == null)
                continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }
    }

    private static void BuildClip(int value)
    {
        var sprites = GetFramePaths(value)
            .Select(AssetDatabase.LoadAssetAtPath<Sprite>)
            .Where(sprite => sprite != null)
            .ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogWarning($"Dice roll frames not found for result {value}.");
            return;
        }

        var clip = new AnimationClip
        {
            frameRate = FrameRate,
            wrapMode = WrapMode.Once,
            legacy = false,
            name = $"DiceRoll_{value}"
        };

        var keyframes = new ObjectReferenceKeyframe[sprites.Length + 1];
        for (var i = 0; i < sprites.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i / FrameRate,
                value = sprites[i]
            };
        }

        keyframes[keyframes.Length - 1] = new ObjectReferenceKeyframe
        {
            time = sprites.Length / FrameRate,
            value = sprites[sprites.Length - 1]
        };

        var binding = new EditorCurveBinding
        {
            path = string.Empty,
            type = typeof(Image),
            propertyName = "m_Sprite"
        };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        var clipPath = GetClipPath(value);
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath) != null)
            AssetDatabase.DeleteAsset(clipPath);

        AssetDatabase.CreateAsset(clip, clipPath);
    }

    private static string[] GetFramePaths(int value)
    {
        var folder = $"{FrameRoot}/Roll_{value}";
        if (!Directory.Exists(folder))
            return Array.Empty<string>();

        return Directory.GetFiles(folder, "*.png")
            .Select(path => path.Replace('\\', '/'))
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
    }

    private static string GetClipPath(int value)
    {
        return $"{ClipRoot}/DiceRoll_{value}.anim";
    }

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder))
            return;

        var parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
        var name = Path.GetFileName(folder);
        if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name))
            return;

        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }
}
