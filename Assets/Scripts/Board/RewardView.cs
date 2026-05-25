using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class RewardView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Button button;

    private RewardData currentReward;

    public event Action<RewardData> Clicked;

    public void SetReward(RewardData reward)
    {
        currentReward = reward;
        Refresh();
    }

    private void OnEnable()
    {
        if (button != null)
            button.onClick.AddListener(HandleClick);

        Refresh();
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);
    }

    private void Refresh()
    {
        if (nameText != null)
            nameText.text = currentReward != null ? currentReward.DisplayName : string.Empty;

        if (descriptionText != null)
            descriptionText.text = currentReward != null ? currentReward.DisplayDescription : string.Empty;

        if (iconImage != null)
        {
            var sprite = currentReward != null ? currentReward.DisplaySprite : null;
            iconImage.sprite = sprite;
            iconImage.enabled = sprite != null;
        }

        if (button != null)
            button.interactable = currentReward != null;
    }

    private void HandleClick()
    {
        if (currentReward != null)
            Clicked?.Invoke(currentReward);
    }
}
