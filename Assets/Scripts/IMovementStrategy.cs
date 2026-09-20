using UnityEngine;

public interface IMovementStrategy
{
    void Move(Transform enemy, MovementState state, float deltaTime, Vector2 boundsMin, Vector2 boundsMax);
}
