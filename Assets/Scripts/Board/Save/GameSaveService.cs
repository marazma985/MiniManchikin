using System;
using System.IO;
using UnityEngine;
/// <summary>
/// Читает, записывает и удаляет файл сохранения партии
/// </summary>

public static class GameSaveService
{
    private const int CurrentVersion = 1;
    private const string SaveFileName = "board_save.json";

    public static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);
    /// <summary>
    /// Проверяет, есть ли файл сохранения партии
    /// </summary>
    public static bool HasSave()
    {
        return File.Exists(SavePath);
    }
    /// <summary>
    /// Сохраняет данные партии или настроек
    /// </summary>
    public static void Save(GameSaveData data)
    {
        if (data == null)
            return;

        try
        {
            data.version = CurrentVersion;
            Directory.CreateDirectory(Application.persistentDataPath);
            var json = JsonUtility.ToJson(data, true);
            var tempPath = SavePath + ".tmp";
            File.WriteAllText(tempPath, json);

            if (File.Exists(SavePath))
                File.Replace(tempPath, SavePath, null);
            else
                File.Move(tempPath, SavePath);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not save game: {exception.Message}");
        }
    }
    /// <summary>
    /// Пытается прочитать файл сохранения и превратить его в данные партии
    /// </summary>
    public static bool TryLoad(out GameSaveData data)
    {
        data = null;

        if (!HasSave())
            return false;

        try
        {
            var json = File.ReadAllText(SavePath);
            data = JsonUtility.FromJson<GameSaveData>(json);
            if (data == null)
                return false;

            if (data.version != CurrentVersion)
            {
                Debug.LogWarning($"Unsupported game save version: {data.version}.");
                data = null;
                return false;
            }

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not load game save: {exception.Message}");
            return false;
        }
    }
    /// <summary>
    /// Удаляет сохраненные или временные данные
    /// </summary>
    public static void DeleteSave()
    {
        try
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);

            var tempPath = SavePath + ".tmp";
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not delete game save: {exception.Message}");
        }
    }
}
