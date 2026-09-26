using UnityEngine;


public abstract class SkillStrategySO : ScriptableObject, ISkillStrategy
{
    [Header("Stats base (compartidos por toda habilidad)")]
    [SerializeField] protected float cooldown = 8f;

    [Header("UI")]
    [Tooltip("Ícono que se muestra en el botón de habilidad cuando este SO está equipado.")]
    [SerializeField] private Sprite icon;

    public float Cooldown => cooldown;
    public Sprite Icon => icon;

    public abstract void Activate(Transform user);

    protected Faction ResolveFaction(Transform user)
    {
        return user.TryGetComponent(out Health health) ? health.Faction : Faction.Player;
    }
}
