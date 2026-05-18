using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class SingleRewardModalView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Text statusText;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button closeButton;

    public event Action AcceptRequested;
    public event Action CloseRequested;

    public void Show(RewardData reward)
    {
        gameObject.SetActive(true);
        SetReward(reward);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetReward(RewardData reward)
    {
        if (nameText != null)
            nameText.text = reward != null ? reward.DisplayName : string.Empty;

        if (descriptionText != null)
            descriptionText.text = GetDescription(reward);

        if (iconImage != null)
        {
            var sprite = reward != null ? reward.DisplaySprite : null;
            iconImage.sprite = sprite;
            iconImage.enabled = sprite != null;
        }
    }

    public void SetAcceptState(bool canAccept, string status)
    {
        if (acceptButton != null)
            acceptButton.interactable = canAccept;

        if (statusText != null)
            statusText.text = status ?? string.Empty;
    }

    private void OnEnable()
    {
        if (acceptButton != null)
            acceptButton.onClick.AddListener(HandleAcceptClicked);

        if (closeButton != null)
            closeButton.onClick.AddListener(HandleCloseClicked);
    }

    private void OnDisable()
    {
        if (acceptButton != null)
            acceptButton.onClick.RemoveListener(HandleAcceptClicked);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(HandleCloseClicked);
    }

    private static string GetDescription(RewardData reward)
    {
        if (reward == null)
            return string.Empty;

        switch (reward.RewardType)
        {
            case RewardType.Card:
                return reward.CardData != null ? reward.CardData.Description : string.Empty;
            case RewardType.Item:
                return reward.ItemData != null ? reward.ItemData.Description : string.Empty;
            default:
                return string.Empty;
        }
    }

    private void HandleAcceptClicked()
    {
        AcceptRequested?.Invoke();
    }

    private void HandleCloseClicked()
    {
        CloseRequested?.Invoke();
    }
}
