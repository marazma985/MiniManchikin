using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Перечисляет варианты effect notification status, которые используются в игровой логике вместо строковых значений
/// </summary>

public enum EffectNotificationStatus
{
    Success,
    Blocked,
    NoEffect,
    Failed
}
/// <summary>
/// Отвечает за игровые события и уведомления, связанные с EventNotificationSystem
/// </summary>
public sealed class EventNotificationSystem : MonoBehaviour
{
    /// <summary>
    /// Отвечает за игровые события и уведомления, связанные с EffectNotificationSetting
    /// </summary>
    [Serializable]
    private sealed class EffectNotificationSetting
    {
        [SerializeField] private EffectType effectType;
        [SerializeField] private Sprite icon;
        [SerializeField] private bool showNotification = true;
        [SerializeField] private string positiveMessageTemplate;
        [SerializeField] private string negativeMessageTemplate;
        [SerializeField] private string blockedMessageTemplate;
        [SerializeField] private string noEffectMessageTemplate;
        [SerializeField] private string failedMessageTemplate;

        public EffectType EffectType => effectType;
        public Sprite Icon => icon;
        public bool ShowNotification => showNotification;
        public string PositiveMessageTemplate => positiveMessageTemplate;
        public string NegativeMessageTemplate => negativeMessageTemplate;
        public string BlockedMessageTemplate => blockedMessageTemplate;
        public string NoEffectMessageTemplate => noEffectMessageTemplate;
        public string FailedMessageTemplate => failedMessageTemplate;
        /// <summary>
        /// Создает экземпляр EffectNotificationSetting и заполняет его начальными данными
        /// </summary>
        public EffectNotificationSetting()
        {
            showNotification = true;
            ApplyDefaultTemplates();
        }
        /// <summary>
        /// Создает экземпляр EffectNotificationSetting и заполняет его начальными данными
        /// </summary>
        public EffectNotificationSetting(EffectType effectType)
        {
            this.effectType = effectType;
            showNotification = true;
            ApplyDefaultTemplates();
        }
        /// <summary>
        /// Применяет изменение к игровому или визуальному состоянию
        /// </summary>
        public void ApplyDefaultTemplates()
        {
            switch (effectType)
            {
                case EffectType.Power:
                    positiveMessageTemplate = "+{value} Power";
                    break;
                case EffectType.HpRestore:
                    positiveMessageTemplate = "+{value} HP";
                    negativeMessageTemplate = "-{value} HP";
                    blockedMessageTemplate = "HP loss blocked";
                    noEffectMessageTemplate = "+{value} HP (full)";
                    break;
                case EffectType.EscapeBonus:
                    positiveMessageTemplate = "+{value} Escape";
                    break;
                case EffectType.Level:
                    positiveMessageTemplate = "+{value} Level";
                    negativeMessageTemplate = "-{value} Level";
                    noEffectMessageTemplate = "{signedValue} Level (no change)";
                    break;
                case EffectType.ChangePosition:
                    positiveMessageTemplate = "Position changed";
                    break;
                case EffectType.GiveCard:
                    positiveMessageTemplate = "Card gained";
                    failedMessageTemplate = "Card not gained";
                    break;
                case EffectType.RemoveCard:
                    positiveMessageTemplate = "Card removed";
                    failedMessageTemplate = "Card not removed";
                    break;
                case EffectType.GiveItem:
                    positiveMessageTemplate = "Item gained";
                    failedMessageTemplate = "Item not gained";
                    break;
                case EffectType.RemoveItem:
                    positiveMessageTemplate = "Item removed";
                    failedMessageTemplate = "Item not removed";
                    break;
            }
        }
    }

    [SerializeField] private RectTransform notificationContainer;
    [SerializeField] private EventNotificationView notificationPrefab;
    [SerializeField] private Sprite defaultIcon;
    [SerializeField, Min(0f)] private float notificationSpacing = 58f;
    [SerializeField] private List<EffectNotificationSetting> notificationSettings = new List<EffectNotificationSetting>();

    /// <summary>
    /// Показывает всплывающее уведомление с иконкой, подходящей под тип эффекта
    /// </summary>
    public void ShowNotification(string message, EffectType effectType)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        var setting = GetSetting(effectType);
        if (setting != null && !setting.ShowNotification)
            return;

        if (notificationContainer == null || notificationPrefab == null)
        {
            Debug.LogWarning("EventNotificationSystem requires notification container and prefab.");
            return;
        }

        var notification = Instantiate(notificationPrefab, notificationContainer);
        if (notification.transform is RectTransform notificationRectTransform)
            notificationRectTransform.anchoredPosition = new Vector2(0f, -GetVisibleNotificationCount() * notificationSpacing);

        notification.gameObject.SetActive(true);
        notification.Show(message, GetIcon(effectType));
    }
    /// <summary>
    /// Показывает нужное окно или визуальное состояние игроку
    /// </summary>
    public void ShowEffectNotification(EffectData effect, EffectNotificationStatus status)
    {
        if (effect == null)
            return;

        ShowEffectNotification(effect.EffectType, effect.Value, status);
    }
    /// <summary>
    /// Показывает нужное окно или визуальное состояние игроку
    /// </summary>
    public void ShowEffectNotification(EffectData effect, EffectNotificationStatus status, int displayValue)
    {
        if (effect == null)
            return;

        ShowEffectNotification(effect.EffectType, displayValue, status);
    }
    /// <summary>
    /// Показывает нужное окно или визуальное состояние игроку
    /// </summary>
    public void ShowEffectNotification(EffectType effectType, int value, EffectNotificationStatus status)
    {
        var setting = GetSetting(effectType);
        if (setting == null || !setting.ShowNotification)
            return;

        var template = GetTemplate(setting, value, status);
        if (string.IsNullOrWhiteSpace(template))
            return;

        ShowNotification(FormatTemplate(template, value), effectType);
    }
    /// <summary>
    /// Возвращает сохраненное или рассчитанное значение
    /// </summary>
    private int GetVisibleNotificationCount()
    {
        var count = 0;
        for (var i = 0; i < notificationContainer.childCount; i++)
        {
            var child = notificationContainer.GetChild(i);
            if (child != null && child.gameObject.activeSelf)
                count++;
        }

        return count;
    }
    /// <summary>
    /// Возвращает сохраненное или рассчитанное значение
    /// </summary>
    private Sprite GetIcon(EffectType effectType)
    {
        var setting = GetSetting(effectType);
        if (setting != null && setting.Icon != null)
            return setting.Icon;

        return defaultIcon;
    }
    /// <summary>
    /// Возвращает сохраненное или рассчитанное значение
    /// </summary>
    private EffectNotificationSetting GetSetting(EffectType effectType)
    {
        if (notificationSettings == null)
            return null;

        for (var i = 0; i < notificationSettings.Count; i++)
        {
            var setting = notificationSettings[i];
            if (setting != null && setting.EffectType == effectType)
                return setting;
        }

        return null;
    }
    /// <summary>
    /// Возвращает сохраненное или рассчитанное значение
    /// </summary>
    private static string GetTemplate(EffectNotificationSetting setting, int value, EffectNotificationStatus status)
    {
        switch (status)
        {
            case EffectNotificationStatus.Blocked:
                return setting.BlockedMessageTemplate;
            case EffectNotificationStatus.NoEffect:
                return setting.NoEffectMessageTemplate;
            case EffectNotificationStatus.Failed:
                return setting.FailedMessageTemplate;
            default:
                return value < 0 ? setting.NegativeMessageTemplate : setting.PositiveMessageTemplate;
        }
    }
    /// <summary>
    /// Формирует текст для отображения или вывода в лог
    /// </summary>
    private static string FormatTemplate(string template, int value)
    {
        var absoluteValue = Mathf.Abs(value);
        var signedValue = value > 0 ? $"+{value}" : value.ToString();
        return template
            .Replace("{value}", absoluteValue.ToString())
            .Replace("{signedValue}", signedValue);
    }
    /// <summary>
    /// Поддерживает корректные значения и ссылки при изменениях в инспекторе Unity
    /// </summary>
    private void OnValidate()
    {
        EnsureDefaultSettings();
    }
    /// <summary>
    /// Инициализирует ссылки и внутреннее состояние до запуска сцены
    /// </summary>
    private void Awake()
    {
        EnsureDefaultSettings();
    }
    /// <summary>
    /// Гарантирует, что нужный объект, ресурс или ссылка существует
    /// </summary>
    private void EnsureDefaultSettings()
    {
        if (notificationSettings == null)
            notificationSettings = new List<EffectNotificationSetting>();

        var effectTypes = (EffectType[])Enum.GetValues(typeof(EffectType));
        for (var i = 0; i < effectTypes.Length; i++)
        {
            if (GetSetting(effectTypes[i]) == null)
                notificationSettings.Add(new EffectNotificationSetting(effectTypes[i]));
        }
    }
}
