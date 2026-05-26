using UnityEngine;

[CreateAssetMenu(menuName = "Board Game/UI/Close Button Sprite Set")]
public sealed class CloseButtonSpriteSet : ScriptableObject
{
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite highlightedSprite;
    [SerializeField] private Sprite pressedSprite;
    [SerializeField, Min(0f)] private float fadeDuration = 0.12f;

    public Sprite NormalSprite => normalSprite;
    public Sprite HighlightedSprite => highlightedSprite;
    public Sprite PressedSprite => pressedSprite;
    public float FadeDuration => fadeDuration;
}
