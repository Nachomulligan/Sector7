using UnityEngine;

[CreateAssetMenu(menuName = "Sector7/Movement/Parabola", fileName = "Movement_Parabola")]
public class ParabolaMovementStrategy : MovementStrategySO
{
    [Header("Arco")]
    [SerializeField] private float duration = 3f;
    [SerializeField] private float verticalDrop = 6f;
    [Tooltip("Desplazamiento horizontal total durante el arco. Positivo = termina más a la derecha de donde spawneó.")]
    [SerializeField] private float horizontalTravel = 4f;
    [Tooltip("Qué tan pronunciado es el arco y hacia qué lado se abre. Positivo = forma \")\", negativo = forma \"(\".")]
    [SerializeField] private float curveStrength = 3f;

    [Header("Después del arco")]
    [SerializeField] private float exitSpeed = 4f;

    public override void Move(Transform enemy, MovementState state, float deltaTime, Vector2 boundsMin, Vector2 boundsMax)
    {
        if (!state.initialized)
        {
            state.spawnPosition = enemy.position;
            state.elapsedTime = 0f;
            state.initialized = true;
        }

        if (state.elapsedTime < duration)
        {
            state.elapsedTime += deltaTime;
            float t = Mathf.Clamp01(state.elapsedTime / duration);

            Vector3 start = state.spawnPosition;
            Vector3 end = start + new Vector3(horizontalTravel, -verticalDrop, 0f);
            Vector3 control = start + new Vector3((horizontalTravel * 0.5f) + curveStrength, -verticalDrop * 0.5f, 0f);

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
