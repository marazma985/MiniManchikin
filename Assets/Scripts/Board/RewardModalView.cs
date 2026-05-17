using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class RewardModalView : MonoBehaviour
{
    [SerializeField] private RewardView[] rewardViews;

    public event Action<RewardData> RewardSelected;

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
}
