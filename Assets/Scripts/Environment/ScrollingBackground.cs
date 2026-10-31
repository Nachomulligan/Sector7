using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ScrollingBackground : MonoBehaviour
{
    [Serializable]
    private sealed class ScrollingLayer
    {
        [SerializeField] private string displayName = "Layer";
        [SerializeField] private SpriteRenderer[] segments;
        [Min(0f)] [SerializeField] private float scrollSpeed = 1f;
        [SerializeField] private bool fitToCamera = true;

        public string DisplayName => displayName;
        public SpriteRenderer[] Segments => segments;
        public float ScrollSpeed => scrollSpeed;
        public bool FitToCamera => fitToCamera;
    }

    [SerializeField] private Camera targetCamera;
    [SerializeField] private ScrollingLayer[] layers;

    private void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (layers == null) return;

        foreach (ScrollingLayer layer in layers)
            if (layer != null) PrepareLayer(layer);
    }

    private void Update()
    {
        if (layers == null) return;
        float deltaTime = Time.deltaTime;

        foreach (ScrollingLayer layer in layers)
        {
            if (layer == null) continue;
            MoveLayer(layer, deltaTime);
            RecycleLayer(layer);
        }
    }

    private void PrepareLayer(ScrollingLayer layer)
    {
        SpriteRenderer[] segments = layer.Segments;
        if (segments == null || segments.Length == 0)
        {
            Debug.LogWarning($"La capa de fondo '{layer.DisplayName}' no tiene segmentos asignados.", this);
            return;
        }

        if (layer.FitToCamera && targetCamera != null && targetCamera.orthographic)
        {
            float cameraHeight = targetCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * targetCamera.aspect;

            foreach (SpriteRenderer segment in segments)
            {
                if (segment == null || segment.sprite == null) continue;
                Vector2 spriteSize = segment.sprite.bounds.size;
                float scale = Mathf.Max(cameraWidth / spriteSize.x, cameraHeight / spriteSize.y);
                segment.transform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        float nextCenterY = targetCamera != null ? targetCamera.transform.position.y : 0f;
        foreach (SpriteRenderer segment in segments)
        {
            if (segment == null) continue;
            float height = segment.bounds.size.y;
            segment.transform.position = new Vector3(
                targetCamera != null ? targetCamera.transform.position.x : segment.transform.position.x,
                nextCenterY,
                segment.transform.position.z);
            nextCenterY += height;
        }
    }

    private static void MoveLayer(ScrollingLayer layer, float deltaTime)
    {
        if (layer.Segments == null) return;
        float step = layer.ScrollSpeed * deltaTime;
        if (step <= 0f) return;

        foreach (SpriteRenderer segment in layer.Segments)
        {
            if (segment != null) segment.transform.position += Vector3.down * step;
        }
    }

    private void RecycleLayer(ScrollingLayer layer)
    {
        SpriteRenderer[] segments = layer.Segments;
        if (segments == null || segments.Length < 2) return;

        float cameraBottom = targetCamera != null
            ? targetCamera.transform.position.y - targetCamera.orthographicSize
            : 0f;

        foreach (SpriteRenderer segment in segments)
        {
            if (segment == null || segment.bounds.max.y > cameraBottom) continue;

            float highestTop = float.MinValue;
            foreach (SpriteRenderer other in segments)
            {
                if (other != null && other != segment)
                    highestTop = Mathf.Max(highestTop, other.bounds.max.y);
            }

            Vector3 position = segment.transform.position;
            position.y = highestTop + segment.bounds.extents.y;
            segment.transform.position = position;
        }
    }
}
