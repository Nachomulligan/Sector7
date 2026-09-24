using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

[DisallowMultipleComponent]
public class PlayerMechaMovement : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera worldCamera;

    [Header("Movimiento")]
    [Tooltip("Velocidad máxima de interpolación hacia el punto de touch (unidades/seg).")]
    [SerializeField] private float followSpeed = 25f;

    [Tooltip("Si es true, el mecha se mueve instantáneamente al punto de touch (sin suavizado).")]
    [SerializeField] private bool snapInstantly = false;

    [Header("Límites de la zona de juego")]
    [Tooltip("Esquina inferior izquierda del área jugable, en coordenadas de mundo.")]
    [SerializeField] private Vector2 boundsMin = new Vector2(-4f, -7f);

    [Tooltip("Esquina superior derecha del área jugable, en coordenadas de mundo.")]
    [SerializeField] private Vector2 boundsMax = new Vector2(4f, 7f);

    [Header("Offset de dedo")]
    [Tooltip("Desplaza el mecha por encima del dedo para que el jugador no lo tape.")]
    [SerializeField] private Vector2 fingerOffset = new Vector2(0f, 1.2f);

    [Header("Interaccioon con UI")]
    [SerializeField] private bool ignoreTouchesOverUI = true;

    private float distanceFromCamera;
    private Vector3 targetPosition;
    private bool isDragging;
    private int activeTouchId = -1;

    private void Awake()
    {
        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }

        distanceFromCamera = Mathf.Abs(
            worldCamera.transform.position.z - transform.position.z
        );

        targetPosition = transform.position;
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        HandleTouchInput();
        ApplyMovement();
    }

    private void HandleTouchInput()
    {
        if (Touch.activeTouches.Count == 0)
        {
            isDragging = false;
            activeTouchId = -1;
            return;
        }

        if (!isDragging)
        {
            foreach (Touch touch in Touch.activeTouches)
            {
                if (touch.phase == TouchPhase.Began && !IsTouchOverUI(touch))
                {
                    activeTouchId = touch.touchId;
                    isDragging = true;
                    UpdateTargetFromScreenPosition(touch.screenPosition);
                    break;
                }
            }
        }

        if (!isDragging)
        {
            return;
        }

        bool stillActive = false;

        foreach (Touch touch in Touch.activeTouches)
        {
            if (touch.touchId != activeTouchId)
            {
                continue;
            }

            stillActive = true;

            if (touch.phase == TouchPhase.Ended ||
                touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
                activeTouchId = -1;
                break;
            }

            UpdateTargetFromScreenPosition(touch.screenPosition);
            break;
        }

        if (!stillActive)
        {
            isDragging = false;
            activeTouchId = -1;
        }
    }
    private bool IsTouchOverUI(Touch touch)
    {
        return ignoreTouchesOverUI &&
               EventSystem.current != null &&
               EventSystem.current.IsPointerOverGameObject(touch.touchId);
    }

    private void UpdateTargetFromScreenPosition(Vector2 screenPosition)
    {
        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(
            new Vector3(screenPosition.x, screenPosition.y, distanceFromCamera)
        );

        worldPosition += (Vector3)fingerOffset;
        worldPosition.z = transform.position.z;

        worldPosition.x = Mathf.Clamp(worldPosition.x, boundsMin.x, boundsMax.x);
        worldPosition.y = Mathf.Clamp(worldPosition.y, boundsMin.y, boundsMax.y);

        targetPosition = worldPosition;
    }

    private void ApplyMovement()
    {
        if (snapInstantly)
        {
            transform.position = targetPosition;
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Vector3 bottomLeft = new Vector3(boundsMin.x, boundsMin.y, transform.position.z);
        Vector3 topRight = new Vector3(boundsMax.x, boundsMax.y, transform.position.z);
        Vector3 bottomRight = new Vector3(boundsMax.x, boundsMin.y, transform.position.z);
        Vector3 topLeft = new Vector3(boundsMin.x, boundsMax.y, transform.position.z);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }
}
