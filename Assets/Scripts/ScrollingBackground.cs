using UnityEngine;
public class ScrollingBackground : MonoBehaviour
{
    [SerializeField] private Transform[] segments;
    [SerializeField] private float scrollSpeed = 2f;
    [SerializeField] private float segmentHeight = 10f;

    private void Update()
    {
        float step = scrollSpeed * Time.deltaTime;

        foreach (Transform segment in segments)
        {
            segment.position += Vector3.down * step;
        }

        foreach (Transform segment in segments)
        {
            if (segment.position.y <= -segmentHeight)
            {
                RecycleToTop(segment);
            }
        }
    }

    private void RecycleToTop(Transform segment)
    {
        float highestY = float.MinValue;

        foreach (Transform other in segments)
        {
            if (other.position.y > highestY)
            {
                highestY = other.position.y;
            }
        }

        segment.position = new Vector3(segment.position.x, highestY + segmentHeight, segment.position.z);
    }
}
