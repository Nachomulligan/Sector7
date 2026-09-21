using UnityEngine;

public abstract class MovementStrategySO : ScriptableObject, IMovementStrategy
{
    public abstract void Move(Transform enemy, MovementState state, float deltaTime, Vector2 boundsMin, Vector2 boundsMax);
}
