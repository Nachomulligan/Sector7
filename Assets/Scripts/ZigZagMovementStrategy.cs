using UnityEngine;

[CreateAssetMenu(menuName = "Sector7/Movement/Zig Zag", fileName = "Movement_ZigZag")]
public class ZigZagMovementStrategy : MovementStrategySO
{
    [SerializeField] private float verticalSpeed = 3f;
    [SerializeField] private float horizontalSpeed = 4f;

    public override void Move(Transform enemy, MovementState state, float deltaTime, Vector2 boundsMin, Vector2 boundsMax)
    {
        if (!state.initialized)
        {
            state.intPhase = 1; // 1 = yendo a la derecha, -1 = yendo a la izquierda
            state.initialized = true;
        }

        Vector3 pos = enemy.position;
        pos.y -= verticalSpeed * deltaTime;
        pos.x += state.intPhase * horizontalSpeed * deltaTime;

        if (pos.x >= boundsMax.x)
        {
            pos.x = boundsMax.x;
            state.intPhase = -1;
        }
        else if (pos.x <= boundsMin.x)
        {
            pos.x = boundsMin.x;
            state.intPhase = 1;
        }

        enemy.position = pos;
    }
}
