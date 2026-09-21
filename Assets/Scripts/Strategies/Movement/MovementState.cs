using UnityEngine;

public class MovementState
{
    public bool initialized;
    public float elapsedTime;
    public Vector3 spawnPosition;
    public Vector2 velocity;
    public int intPhase;

    public void Reset()
    {
        initialized = false;
        elapsedTime = 0f;
        spawnPosition = Vector3.zero;
        velocity = Vector2.zero;
        intPhase = 0;
    }
}
