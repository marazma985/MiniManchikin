using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class RewardModalView : MonoBehaviour
{
    [SerializeField] private RewardView[] rewardViews;
    [SerializeField] private Button closeButton;

    public event Action<RewardData> RewardSelected;
    public event Action CloseRequested;

    public void Show(IReadOnlyList<RewardData> rewards)
    {
        gameObject.SetActive(true);
        Refresh(rewards);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(HandleCloseClicked);
            closeButton.onClick.AddListener(HandleCloseClicked);
        }

        if (rewardViews == null)
            return;

        for (var i = 0; i < rewardViews.Length; i++)
        {
            var rewardView = rewardViews[i];
            if (rewardView == null)
                continue;

            rewardView.Clicked -= HandleRewardClicked;
            rewardView.Clicked += HandleRewardClicked;
        }
    }

    private void Unsubscribe()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(HandleCloseClicked);

        if (rewardViews == null)
            return;

        for (var i = 0; i < rewardViews.Length; i++)
        {
            var rewardView = rewardViews[i];
            if (rewardView != null)
                rewardView.Clicked -= HandleRewardClicked;
        }
    }

    private void Refresh(IReadOnlyList<RewardData> rewards)
    {
        if (rewardViews == null)
            return;

        for (var i = 0; i < rewardViews.Length; i++)
        {
            var rewardView = rewardViews[i];
            if (rewardView == null)
                continue;

            var reward = rewards != null && i < rewards.Count ? rewards[i] : null;
            rewardView.SetReward(reward);
            rewardView.gameObject.SetActive(reward != null);
        }
    }

    private void HandleRewardClicked(RewardData reward)
    {
        RewardSelected?.Invoke(reward);
    }

    private void HandleCloseClicked()
    {
        CloseRequested?.Invoke();
    }
}
