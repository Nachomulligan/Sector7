using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class CompanionFollower : MonoBehaviour
{
    private Transform target;
    private Vector2 offset;
    private float smoothness;

    public void Initialize(Transform followTarget, CompanionPresetSO preset)
    {
        target = followTarget;
        offset = preset.FollowOffset;
        smoothness = preset.FollowSmoothness;
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = preset.Appearance;
        renderer.color = preset.AppearanceColor;
        transform.position = (Vector2)target.position + offset;
    }

    private void LateUpdate()
    {
        if (target == null) return;
        Vector2 destination = (Vector2)target.position + offset;
        float t = smoothness <= 0f ? 1f : 1f - Mathf.Exp(-smoothness * Time.deltaTime);
        transform.position = Vector2.Lerp(transform.position, destination, t);
    }
}
