using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class CompanionFollower : MonoBehaviour
{
    private Transform target;
    private CompanionMovementStrategySO movementStrategy;
    private Vector2 offset;
    private float smoothness;
    private float snapDistance;
    private float elapsedTime;
    private float phaseRadians;

    public void Initialize(Transform followTarget, CompanionPresetSO preset)
    {
        if (followTarget == null || preset == null)
        {
            enabled = false;
            return;
        }

        enabled = true;
        target = followTarget;
        offset = preset.FollowOffset;
        smoothness = preset.FollowSmoothness;
        snapDistance = preset.SnapDistance;
        phaseRadians = preset.MotionPhaseRadians;
        movementStrategy = preset.MovementStrategy;
        elapsedTime = 0f;

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (preset.Appearance != null) renderer.sprite = preset.Appearance;
        renderer.color = preset.AppearanceColor;
        renderer.sortingLayerName = preset.SortingLayerName;
        renderer.sortingOrder = preset.SortingOrder;
        transform.localScale = Vector3.one * preset.VisualScale;
        transform.position = Destination();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        float deltaTime = Time.deltaTime;
        elapsedTime += deltaTime;
        Vector2 destination = Destination();
        Vector2 current = transform.position;

        if (snapDistance > 0f && (destination - current).sqrMagnitude > snapDistance * snapDistance)
        {
            transform.position = destination;
            return;
        }

        float t = smoothness <= 0f ? 1f : 1f - Mathf.Exp(-smoothness * deltaTime);
        transform.position = Vector2.Lerp(current, destination, t);
    }

    private Vector2 Destination()
    {
        Vector2 drift = movementStrategy != null
            ? movementStrategy.EvaluateOffset(elapsedTime, phaseRadians)
            : Vector2.zero;
        return (Vector2)target.position + offset + drift;
    }
}
