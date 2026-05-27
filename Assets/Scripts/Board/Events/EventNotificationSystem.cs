using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Результат применения эффекта для всплывающей подсказки: успешно, заблокировано или без эффекта
/// </summary>

public enum EffectNotificationStatus
{
    Success,
    Blocked,
    NoEffect,
    Failed
}
/// <summary>
/// Создает всплывающие подсказки о событиях, бонусах, уроне и других изменениях
/// </summary>
public sealed class EventNotificationSystem : MonoBehaviour
{
    /// <summary>
    /// Настройки одной всплывающей подсказки: иконка и тексты для разных исходов
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
        /// Создает пустую настройку подсказки для заполнения в инспекторе
        /// </summary>
        public EffectNotificationSetting()
        {
            showNotification = true;
            ApplyDefaultTemplates();
        }
        /// <summary>
        /// Создает настройку подсказки для указанного типа эффекта
        /// </summary>
        public EffectNotificationSetting(EffectType effectType)
        {
            this.effectType = effectType;
            showNotification = true;
            ApplyDefaultTemplates();
        }
        /// <summary>
        /// Заполняет стандартные тексты подсказок для эффектов клеток
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
    /// Показывает всплывающую подсказку игроку
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
    /// Показывает подсказку о полученном или потерянном эффекте
    /// </summary>
    public void ShowEffectNotification(EffectData effect, EffectNotificationStatus status)
    {
        if (effect == null)
            return;

        ShowEffectNotification(effect.EffectType, effect.Value, status);
    }
    /// <summary>
    /// Показывает подсказку о полученном или потерянном эффекте
    /// </summary>
    public void ShowEffectNotification(EffectData effect, EffectNotificationStatus status, int displayValue)
    {
        if (effect == null)
            return;

        ShowEffectNotification(effect.EffectType, displayValue, status);
    }
    /// <summary>
    /// Показывает подсказку о полученном или потерянном эффекте
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
    /// Считает, сколько подсказок сейчас видно на экране
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
    /// Возвращает иконку для указанного типа эффекта
    /// </summary>
    private Sprite GetIcon(EffectType effectType)
    {
        var setting = GetSetting(effectType);
        if (setting != null && setting.Icon != null)
            return setting.Icon;

        return defaultIcon;
    }
    /// <summary>
    /// Возвращает настройки подсказки для указанного типа эффекта
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
    /// Выбирает текстовый шаблон подсказки для плюса или минуса эффекта
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
    /// Подставляет число эффекта в текстовый шаблон подсказки
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
    /// Заполняет стандартные шаблоны подсказок после правок в инспекторе
    /// </summary>
    private void OnValidate()
    {
        EnsureDefaultSettings();
    }
    /// <summary>
    /// Готовит контейнер, в котором появляются подсказки событий
    /// </summary>
    private void Awake()
    {
        EnsureDefaultSettings();
    }
    /// <summary>
    /// Создает или находит то, без чего объект не сможет работать
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
