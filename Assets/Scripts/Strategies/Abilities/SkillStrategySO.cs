using UnityEngine;


public abstract class SkillStrategySO : ScriptableObject, ISkillStrategy
{
    [Header("Stats base (compartidos por toda habilidad)")]
    [SerializeField] protected float cooldown = 8f;

    public float Cooldown => cooldown;

    public abstract void Activate(Transform user);

    protected Faction ResolveFaction(Transform user)
    {
        return user.TryGetComponent(out Health health) ? health.Faction : Faction.Player;
    }
}
