using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Sector7/Movement/Parabola", fileName = "Movement_Parabola")]
public class ParabolaMovementStrategy : MovementStrategySO
{
    [Header("Parábola completa")]
    [SerializeField] private float duration = 3f;
    [SerializeField] private float verticalDrop = 6f;
    [FormerlySerializedAs("curveStrength")]
    [Tooltip("Distancia horizontal máxima desde el eje inicial. Positivo abre hacia la derecha y negativo hacia la izquierda.")]
    [SerializeField] private float horizontalBulge = 3f;

    [Header("Después de la parábola")]
    [SerializeField] private float exitSpeed = 4f;

    public override void Move(Transform enemy, MovementState state, float deltaTime, Vector2 boundsMin, Vector2 boundsMax)
    {
        if (!state.initialized)
        {
            float fittedBulge = FitBulgeToBounds(horizontalBulge, boundsMin.x, boundsMax.x);
            float minStartX = boundsMin.x - Mathf.Min(0f, fittedBulge);
            float maxStartX = boundsMax.x - Mathf.Max(0f, fittedBulge);
            float startX = Mathf.Clamp(enemy.position.x, minStartX, maxStartX);

            state.spawnPosition = new Vector3(startX, enemy.position.y, enemy.position.z);
            state.velocity.x = fittedBulge;
            state.elapsedTime = 0f;
            state.initialized = true;
        }

        if (state.elapsedTime < duration)
        {
            state.elapsedTime += deltaTime;
            float t = Mathf.Clamp01(state.elapsedTime / duration);

            Vector3 start = state.spawnPosition;
            Vector3 end = start + new Vector3(0f, -verticalDrop, 0f);
            Vector3 control = start + new Vector3(state.velocity.x * 2f, -verticalDrop * 0.5f, 0f);

            enemy.position = QuadraticBezier(start, control, end, t);
        }
        else
        {
            enemy.position += Vector3.down * exitSpeed * deltaTime;
        }
    }

    private static float FitBulgeToBounds(float requestedBulge, float minX, float maxX)
    {
        float maxMagnitude = Mathf.Max(0f, maxX - minX);
        return Mathf.Clamp(requestedBulge, -maxMagnitude, maxMagnitude);
    }

    private static Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;
        return (u * u * p0) + (2f * u * t * p1) + (t * t * p2);
    }
}
