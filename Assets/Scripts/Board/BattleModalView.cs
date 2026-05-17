using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BattleModalView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image playerPortraitImage;
    [SerializeField] private TextMeshProUGUI playerPowerText;
    [SerializeField] private TextMeshProUGUI playerTotalText;
    [SerializeField] private TextMeshProUGUI enemyNameText;
    [SerializeField] private Image enemyPortraitImage;
    [SerializeField] private TextMeshProUGUI enemyPowerText;
    [SerializeField] private TextMeshProUGUI enemyTotalText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    [SerializeField] private Button resolveButton;

    public event Action ResolveRequested;

    public void Show(BattleModalData data)
    {
        if (data == null)
            return;

        SetText(playerNameText, data.PlayerName);
        // Player portrait is a static scene UI image and should not be overwritten per battle.
        SetText(playerPowerText, FormatEntries(data.PlayerPowerEntries));
        SetText(playerTotalText, $"Total: {data.PlayerTotalPower}");

        SetText(enemyNameText, data.EnemyName);
        SetImage(enemyPortraitImage, data.EnemySprite);
        SetText(enemyPowerText, FormatEntries(data.EnemyPowerEntries));
        SetText(enemyTotalText, $"Total: {data.EnemyTotalPower}");
        UpdateState("Battle started", "Resolve Battle");

        gameObject.SetActive(true);
    }

    public void UpdateState(string status, string buttonText)
    {
        SetText(statusText, status);
        SetText(actionButtonText, buttonText);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (resolveButton != null)
            resolveButton.onClick.AddListener(HandleResolveClicked);
    }

    private void OnDisable()
    {
        if (resolveButton != null)
            resolveButton.onClick.RemoveListener(HandleResolveClicked);
    }

    private void HandleResolveClicked()
    {
        ResolveRequested?.Invoke();
    }

    private static void SetText(TextMeshProUGUI text, string value)
    {
        if (text != null)
            text.text = value;
    }

    private static void SetImage(Image image, Sprite sprite)
    {
        if (image == null)
            return;

        image.sprite = sprite;
        image.enabled = sprite != null;
    }

    private static string FormatEntries(IReadOnlyList<BattlePowerEntry> entries)
    {
        if (entries == null || entries.Count == 0)
            return string.Empty;

        var builder = new StringBuilder();
        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            builder.Append(entry.Label);
            builder.Append(": ");
            builder.Append(entry.Value);

            if (i < entries.Count - 1)
                builder.AppendLine();
        }

        return builder.ToString();
    }
}
