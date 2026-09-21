using UnityEngine;

[CreateAssetMenu(menuName = "Sector7/Movement/Sine Wave", fileName = "Movement_SineWave")]
public class SineWaveMovementStrategy : MovementStrategySO
{
    [SerializeField] private float verticalSpeed = 3f;
    [SerializeField] private float amplitude = 2f;
    [SerializeField] private float frequency = 2f;

    public override void Move(Transform enemy, MovementState state, float deltaTime, Vector2 boundsMin, Vector2 boundsMax)
    {
        if (!state.initialized)
        {
            float availableHalfWidth = Mathf.Max(0f, (boundsMax.x - boundsMin.x) * 0.5f);
            float fittedAmplitude = Mathf.Sign(amplitude) * Mathf.Min(Mathf.Abs(amplitude), availableHalfWidth);
            float amplitudeMagnitude = Mathf.Abs(fittedAmplitude);
            float centerX = Mathf.Clamp(
                enemy.position.x,
                boundsMin.x + amplitudeMagnitude,
                boundsMax.x - amplitudeMagnitude
            );

            state.spawnPosition = new Vector3(centerX, enemy.position.y, enemy.position.z);
            state.velocity.x = fittedAmplitude;
            state.elapsedTime = 0f;
            state.initialized = true;
        }

        state.elapsedTime += deltaTime;

        Vector3 pos = enemy.position;
        pos.y -= verticalSpeed * deltaTime;
        pos.x = state.spawnPosition.x + (Mathf.Sin(state.elapsedTime * frequency) * state.velocity.x);

        enemy.position = pos;
    }
}
