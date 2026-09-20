using UnityEngine;

[CreateAssetMenu(menuName = "Sector7/Movement/Hover", fileName = "Movement_Hover")]
public class HoverMovementStrategy : MovementStrategySO
{
    [Header("Aproximación")]
    [SerializeField] private float approachSpeed = 3f;
    [Tooltip("Cuánto desciende (en unidades, valor negativo) antes de quedarse a flotar, relativo a su Y de spawn.")]
    [SerializeField] private float hoverYOffset = -4f;

    [Header("Patrulla en el punto de hover")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float patrolRange = 2f;

    private const int PhaseApproach = 0;
    private const int PhaseHover = 1;

    public override void Move(Transform enemy, MovementState state, float deltaTime, Vector2 boundsMin, Vector2 boundsMax)
    {
        if (!state.initialized)
        {
            state.spawnPosition = enemy.position;
            state.intPhase = PhaseApproach;
            state.initialized = true;
        }

        float hoverY = state.spawnPosition.y + hoverYOffset;
        Vector3 pos = enemy.position;

        if (state.intPhase == PhaseApproach)
        {
            pos.y -= approachSpeed * deltaTime;

            if (pos.y <= hoverY)
            {
                pos.y = hoverY;
                state.intPhase = PhaseHover;
                state.velocity = new Vector2(1f, 0f);
            }
        }
        else
        {
            pos.x += state.velocity.x * patrolSpeed * deltaTime;

            float distanceFromSpawnX = pos.x - state.spawnPosition.x;

            if (Mathf.Abs(distanceFromSpawnX) >= patrolRange)
            {
                state.velocity.x *= -1f;
            }
        }

        enemy.position = pos;
    }
}
