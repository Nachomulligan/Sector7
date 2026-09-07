using UnityEngine;


public interface ISkillStrategy
{
    void Activate(Transform user);
    float Cooldown { get; }
}
