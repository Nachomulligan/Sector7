using UnityEngine;

[CreateAssetMenu(menuName = "Sector7/Movement/Dash", fileName = "Movement_Dash")]
public class DashMovementStrategy : MovementStrategySO
{
    [Header("Fase de aproximación")]
    [SerializeField] private float approachSpeed = 2f;
    [SerializeField] private float approachDuration = 1.5f;

    [Header("Fase de dash")]
    [SerializeField] private float dashSpeed = 14f;
    [SerializeField] private bool aimAtPlayerOnDash = true;

    private const int PhaseApproach = 0;
    private const int PhaseDash = 1;

    public override void Move(Transform enemy, MovementState state, float deltaTime, Vector2 boundsMin, Vector2 boundsMax)
    {
        if (!state.initialized)
        {
            state.intPhase = PhaseApproach;
            state.elapsedTime = 0f;
            state.initialized = true;
        }

        if (state.intPhase == PhaseApproach)
        {
            state.elapsedTime += deltaTime;
            enemy.position += Vector3.down * approachSpeed * deltaTime;

            if (state.elapsedTime >= approachDuration)
            {
                state.intPhase = PhaseDash;
                state.velocity = ResolveDashDirection(enemy) * dashSpeed;
            }
        }
        else
        {
            enemy.position += (Vector3)(state.velocity * deltaTime);
        }
    }

    private Vector2 ResolveDashDirection(Transform enemy)
    {
        if (aimAtPlayerOnDash)
        {
            Health[] allHealth = Object.FindObjectsOfType<Health>();

            foreach (Health health in allHealth)
            {
                if (health.Faction == Faction.Player && health.IsAlive)
                {
                    return ((Vector2)health.transform.position - (Vector2)enemy.position).normalized;
                }
            }
        }

        return Vector2.down;
    }
}
