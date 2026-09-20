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
            state.spawnPosition = enemy.position;
            state.elapsedTime = 0f;
            state.initialized = true;
        }

        state.elapsedTime += deltaTime;

        Vector3 pos = enemy.position;
        pos.y -= verticalSpeed * deltaTime;
        pos.x = state.spawnPosition.x + (Mathf.Sin(state.elapsedTime * frequency) * amplitude);

        enemy.position = pos;
    }
}
