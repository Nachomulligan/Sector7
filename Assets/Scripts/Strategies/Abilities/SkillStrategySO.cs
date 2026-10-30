using UnityEngine;

public abstract class SkillStrategySO : ScriptableObject, ISkillStrategy, IItemPresentationSource
{
    [Header("Presentación en UI")]
    [SerializeField] private string displayName;
    [TextArea(2, 5)] [SerializeField] private string description;
    [Tooltip("Icono mostrado tanto en el inventario como en el botón de habilidad.")]
    [SerializeField] private Sprite icon;
    [SerializeField] private Color iconColor = Color.white;

    [Header("Stats base (compartidos por toda habilidad)")]
    [SerializeField] protected float cooldown = 8f;

    public float Cooldown => cooldown;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName)
        ? name.Replace("Ability_", string.Empty) : displayName;
    public string Description => string.IsNullOrWhiteSpace(description)
        ? "Habilidad especial configurable." : description;
    public Sprite Icon => icon;
    public Color IconColor => iconColor;

    public abstract void Activate(Transform user);

    protected Faction ResolveFaction(Transform user)
    {
        return user.TryGetComponent(out Health health) ? health.Faction : Faction.Player;
    }
}
