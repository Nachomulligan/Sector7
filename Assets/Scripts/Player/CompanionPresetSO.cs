using UnityEngine;

[CreateAssetMenu(menuName = "Sector7/Companions/Companion Preset", fileName = "Companion_")]
public sealed class CompanionPresetSO : ScriptableObject
{
    [Header("Identidad visual")]
    [SerializeField] private string displayName;
    [SerializeField] private CompanionFollower visualPrefab;
    [Tooltip("Sprite aplicado al prefab visual cuando este companion está equipado.")]
    [SerializeField] private Sprite appearance;
    [SerializeField] private Color appearanceColor = Color.cyan;
    [Tooltip("Escala uniforme aplicada al visual instanciado. No depende de la escala del prefab.")]
    [Min(0.01f)] [SerializeField] private float visualScale = 0.18f;
    [SerializeField] private string sortingLayerName = "Entities";
    [SerializeField] private int sortingOrder = 2;

    [Header("Seguimiento")]
    [Tooltip("Posición base respecto al mecha. Un valor Y negativo lo coloca detrás.")]
    [SerializeField] private Vector2 followOffset = new Vector2(0.8f, -0.8f);
    [Min(0f)] [SerializeField] private float followSmoothness = 10f;
    [Min(0f)] [SerializeField] private float snapDistance = 5f;
    [Range(0f, 360f)] [SerializeField] private float motionPhaseDegrees;
    [SerializeField] private CompanionMovementStrategySO movementStrategy;

    [Header("Bonificación pasiva")]
    [SerializeField] private StatModifierStrategySO modifier;

    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    public CompanionFollower VisualPrefab => visualPrefab;
    public Sprite Appearance => appearance;
    public Color AppearanceColor => appearanceColor;
    public float VisualScale => visualScale;
    public string SortingLayerName => sortingLayerName;
    public int SortingOrder => sortingOrder;
    public Vector2 FollowOffset => followOffset;
    public float FollowSmoothness => followSmoothness;
    public float SnapDistance => snapDistance;
    public float MotionPhaseRadians => motionPhaseDegrees * Mathf.Deg2Rad;
    public CompanionMovementStrategySO MovementStrategy => movementStrategy;
    public StatModifierStrategySO Modifier => modifier;
}
