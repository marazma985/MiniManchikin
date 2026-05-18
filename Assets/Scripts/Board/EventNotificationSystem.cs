using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class EventNotificationSystem : MonoBehaviour
{
    [Serializable]
    private sealed class EffectIcon
    {
        [SerializeField] private EffectType effectType;
        [SerializeField] private Sprite icon;

        public EffectType EffectType => effectType;
        public Sprite Icon => icon;
    }

    [SerializeField] private RectTransform notificationContainer;
    [SerializeField] private EventNotificationView notificationPrefab;
    [SerializeField] private Sprite defaultIcon;
    [SerializeField, Min(0f)] private float notificationSpacing = 58f;
    [SerializeField] private List<EffectIcon> effectIcons = new List<EffectIcon>();

    public void ShowNotification(string message, EffectType effectType)
    {
        if (string.IsNullOrWhiteSpace(message))
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

    private Sprite GetIcon(EffectType effectType)
    {
        if (effectIcons != null)
        {
            for (var i = 0; i < effectIcons.Count; i++)
            {
                var effectIcon = effectIcons[i];
                if (effectIcon != null && effectIcon.EffectType == effectType && effectIcon.Icon != null)
                    return effectIcon.Icon;
            }
        }

        return defaultIcon;
    }

    private void OnValidate()
    {
        if (effectIcons == null)
            effectIcons = new List<EffectIcon>();
    }
}
