using UnityEngine;

/// <summary>
/// Calcula únicamente el desplazamiento visual adicional del companion.
/// El seguimiento del objetivo y su estado permanecen en CompanionFollower.
/// </summary>
public abstract class CompanionMovementStrategySO : ScriptableObject
{
    public abstract Vector2 EvaluateOffset(float elapsedTime, float phaseRadians);
}
