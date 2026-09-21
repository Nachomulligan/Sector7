using UnityEngine;

[ExecuteAlways]
public class PlayAreaBoundsTester : MonoBehaviour
{
    [Header("Camara de referencia")]
    [SerializeField] private Camera targetCamera;

    [Header("Relaciones de aspecto a soportar (ancho / alto)")]
    [Tooltip("Dispositivo mas 'ancho' a soportar. Ej: tablet 4:3 = 0.75.")]
    [SerializeField] private float widestAspect = 0.75f;
    [Tooltip("Dispositivo mas 'angosto' a soportar. Ej: celular 20:9 = 0.45.")]
    [SerializeField] private float narrowestAspect = 0.45f;

    [Header("Margen adicional (radio del sprite, colchon visual, etc.)")]
    [SerializeField] private float horizontalMargin = 0.5f;
    [SerializeField] private float verticalMargin = 0.5f;

    [Header("Bounds bajo prueba (los que copiarías a tus componentes)")]
    public Vector2 boundsMin = new Vector2(-3f, -6.5f);
    public Vector2 boundsMax = new Vector2(3f, 6.5f);

    [Header("Visualización")]
    [SerializeField] private bool alwaysVisible = true;
    [SerializeField] private Color widestColor = new Color(1f, 1f, 0f, 0.6f);
    [SerializeField] private Color narrowestColor = new Color(0f, 1f, 1f, 0.6f);
    [SerializeField] private Color safeZoneColor = new Color(0f, 1f, 0f, 0.9f);
    [SerializeField] private Color testedBoundsColor = new Color(1f, 0f, 1f, 1f);

    public Camera TargetCamera => targetCamera != null ? targetCamera : Camera.main;

    private void OnDrawGizmos()
    {
        if (alwaysVisible)
        {
            DrawAll();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!alwaysVisible)
        {
            DrawAll();
        }
    }

    private void DrawAll()
    {
        Camera cam = TargetCamera;

        if (cam == null || !cam.orthographic)
        {
            return;
        }

        float orthoSize = cam.orthographicSize;
        Vector3 center = cam.transform.position;
        float z = transform.position.z;

        DrawAspectRect(center, orthoSize, widestAspect, z, widestColor);
        DrawAspectRect(center, orthoSize, narrowestAspect, z, narrowestColor);
        DrawSafeZone(center, orthoSize, z);
        DrawTestedBounds(z);
    }

    private void DrawAspectRect(Vector3 center, float orthoSize, float aspect, float z, Color color)
    {
        float halfWidth = orthoSize * aspect;
        Vector3 c = new Vector3(center.x, center.y, z);
        Vector3 size = new Vector3(halfWidth * 2f, orthoSize * 2f, 0f);

        Gizmos.color = color;
        Gizmos.DrawWireCube(c, size);
    }

    private void DrawSafeZone(Vector3 center, float orthoSize, float z)
    {
        float safeHalfWidth = orthoSize * Mathf.Min(widestAspect, narrowestAspect);
        Vector3 c = new Vector3(center.x, center.y, z);
        Vector3 size = new Vector3(safeHalfWidth * 2f, orthoSize * 2f, 0f);

        Gizmos.color = safeZoneColor;
        Gizmos.DrawWireCube(c, size);
    }

    private void DrawTestedBounds(float z)
    {
        Vector3 center = new Vector3((boundsMin.x + boundsMax.x) * 0.5f, (boundsMin.y + boundsMax.y) * 0.5f, z);
        Vector3 size = new Vector3(boundsMax.x - boundsMin.x, boundsMax.y - boundsMin.y, 0f);

        Gizmos.color = testedBoundsColor;
        Gizmos.DrawWireCube(center, size);
    }


    [ContextMenu("Ajustar Bounds a la Zona Segura")]
    private void FitBoundsToSafeZone()
    {
        Camera cam = TargetCamera;

        if (cam == null || !cam.orthographic)
        {
            Debug.LogWarning("PlayAreaBoundsTester: no se encontro una camara ortografica para calcular la zona segura.");
            return;
        }

        float safeHalfWidth = (cam.orthographicSize * Mathf.Min(widestAspect, narrowestAspect)) - horizontalMargin;
        float safeHalfHeight = cam.orthographicSize - verticalMargin;

        boundsMin = new Vector2(-safeHalfWidth, -safeHalfHeight);
        boundsMax = new Vector2(safeHalfWidth, safeHalfHeight);
    }
}