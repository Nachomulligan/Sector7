using UnityEngine;

[CreateAssetMenu(
    menuName = "Sector7/Companions/Movement/Sine Drift",
    fileName = "CompanionMovement_SineDrift")]
public sealed class SineDriftCompanionMovementSO : CompanionMovementStrategySO
{
    [Header("Amplitud")]
    [Min(0f)] [SerializeField] private float horizontalAmplitude = 0.35f;
    [Min(0f)] [SerializeField] private float verticalAmplitude = 0.18f;

    [Header("Frecuencia")]
    [Min(0f)] [SerializeField] private float horizontalCyclesPerSecond = 0.65f;
    [Min(0f)] [SerializeField] private float verticalCyclesPerSecond = 0.9f;
    [SerializeField] private bool verticalUsesCosine = true;

    public override Vector2 EvaluateOffset(float elapsedTime, float phaseRadians)
    {
        float horizontalAngle = elapsedTime * horizontalCyclesPerSecond * Mathf.PI * 2f + phaseRadians;
        float verticalAngle = elapsedTime * verticalCyclesPerSecond * Mathf.PI * 2f + phaseRadians;
        float x = Mathf.Sin(horizontalAngle) * horizontalAmplitude;
        float yWave = verticalUsesCosine ? Mathf.Cos(verticalAngle) : Mathf.Sin(verticalAngle);
        return new Vector2(x, yWave * verticalAmplitude);
    }
}
