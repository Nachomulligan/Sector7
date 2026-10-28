using System;
using UnityEngine;

[Serializable]
public struct RarityVisualStyle
{
    [SerializeField] private Color background;
    [SerializeField] private Color frame;
    [SerializeField] private Color text;

    public Color Background => background;
    public Color Frame => frame;
    public Color Text => text;

    public RarityVisualStyle(Color background, Color frame, Color text)
    {
        this.background = background;
        this.frame = frame;
        this.text = text;
    }
}

[CreateAssetMenu(menuName = "Sector7/UI/Rarity Visual Config", fileName = "RarityVisualConfig")]
public sealed class RarityVisualConfigSO : ScriptableObject
{
    [SerializeField] private RarityVisualStyle common = new RarityVisualStyle(
        new Color(0.22f, 0.28f, 0.36f), new Color(0.62f, 0.7f, 0.8f), Color.white);
    [SerializeField] private RarityVisualStyle rare = new RarityVisualStyle(
        new Color(0.08f, 0.25f, 0.55f), new Color(0.12f, 0.58f, 1f), new Color(0.35f, 0.78f, 1f));
    [SerializeField] private RarityVisualStyle epic = new RarityVisualStyle(
        new Color(0.28f, 0.08f, 0.48f), new Color(0.72f, 0.24f, 1f), new Color(0.82f, 0.52f, 1f));
    [SerializeField] private RarityVisualStyle legendary = new RarityVisualStyle(
        new Color(0.48f, 0.22f, 0.04f), new Color(1f, 0.66f, 0.08f), new Color(1f, 0.78f, 0.22f));

    public RarityVisualStyle Get(GachaRarity rarity)
    {
        switch (rarity)
        {
            case GachaRarity.Rare: return rare;
            case GachaRarity.Epic: return epic;
            case GachaRarity.Legendary: return legendary;
            default: return common;
        }
    }
}
