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

    [Header("Audio")]
    [Tooltip("Sonido reproducido cuando la habilidad se activa correctamente.")]
    [SerializeField] private AudioClip activationSound;
    [Range(0f, 1f)] [SerializeField] private float activationSoundVolume = 1f;

    public float Cooldown => cooldown;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName)
        ? name.Replace("Ability_", string.Empty) : displayName;
    public string Description => string.IsNullOrWhiteSpace(description)
        ? "Habilidad especial configurable." : description;
    public Sprite Icon => icon;
    public Color IconColor => iconColor;

    public void Activate(Transform user)
    {
        if (user == null || !TryActivateEffect(user)) return;
        if (ServiceLocator.Instance.TryGet(out AudioService audioService))
            audioService.PlaySfx(activationSound, activationSoundVolume);
    }

    protected abstract bool TryActivateEffect(Transform user);

    protected Faction ResolveFaction(Transform user)
    {
        return user.TryGetComponent(out Health health) ? health.Faction : Faction.Player;
    }
}
