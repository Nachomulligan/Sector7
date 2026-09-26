using UnityEngine;

[CreateAssetMenu(menuName = "Sector7/Companions/Companion Preset", fileName = "Companion_")]
public sealed class CompanionPresetSO : ScriptableObject
{
    [SerializeField] private string displayName;
    [SerializeField] private CompanionFollower visualPrefab;
    [SerializeField] private Sprite appearance;
    [SerializeField] private Color appearanceColor = Color.cyan;
    [SerializeField] private Vector2 followOffset = new Vector2(0.8f, -0.8f);
    [Min(0f)] [SerializeField] private float followSmoothness = 10f;
    [SerializeField] private StatModifierStrategySO modifier;

    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public CompanionFollower VisualPrefab => visualPrefab;
    public Sprite Appearance => appearance;
    public Color AppearanceColor => appearanceColor;
    public Vector2 FollowOffset => followOffset;
    public float FollowSmoothness => followSmoothness;
    public StatModifierStrategySO Modifier => modifier;
}
