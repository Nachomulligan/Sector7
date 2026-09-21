using UnityEngine;

[CreateAssetMenu(menuName = "Sector7/Movement/Half Parabola", fileName = "Movement_HalfParabola")]
public class HalfParabolaMovementStrategy : MovementStrategySO
{
    [Header("Media parábola")]
    [SerializeField] private float duration = 3f;
    [SerializeField] private float verticalDrop = 6f;
    [Tooltip("Desplazamiento horizontal total. Positivo termina a la derecha y negativo a la izquierda.")]
    [SerializeField] private float horizontalTravel = 4f;
    [SerializeField] private float curveStrength = 3f;

    [Header("Después del arco")]
    [SerializeField] private float exitSpeed = 4f;

    public override void Move(Transform enemy, MovementState state, float deltaTime, Vector2 boundsMin, Vector2 boundsMax)
    {
        if (!state.initialized)
        {
            float startX = Mathf.Clamp(enemy.position.x, boundsMin.x, boundsMax.x);
            float endX = Mathf.Clamp(startX + horizontalTravel, boundsMin.x, boundsMax.x);

            state.spawnPosition = new Vector3(startX, enemy.position.y, enemy.position.z);
            state.velocity.x = endX - startX;
            state.elapsedTime = 0f;
            state.initialized = true;
        }

        if (state.elapsedTime < duration)
        {
            state.elapsedTime += deltaTime;
            float t = Mathf.Clamp01(state.elapsedTime / duration);

            Vector3 start = state.spawnPosition;
            Vector3 end = start + new Vector3(state.velocity.x, -verticalDrop, 0f);
            float requestedControlX = start.x + (state.velocity.x * 0.5f) + curveStrength;
            float controlX = Mathf.Clamp(requestedControlX, boundsMin.x, boundsMax.x);
            Vector3 control = new Vector3(controlX, start.y - (verticalDrop * 0.5f), start.z);

            enemy.position = QuadraticBezier(start, control, end, t);
        }
        else
        {
            enemy.position += Vector3.down * exitSpeed * deltaTime;
        }
    }

    private static Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;
        return (u * u * p0) + (2f * u * t * p1) + (t * t * p2);
    }
}
