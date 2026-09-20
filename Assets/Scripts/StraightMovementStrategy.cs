using UnityEngine;

[CreateAssetMenu(menuName = "Sector7/Movement/Straight", fileName = "Movement_Straight")]
public class StraightMovementStrategy : MovementStrategySO
{
    [SerializeField] private float speed = 3f;

    public override void Move(Transform enemy, MovementState state, float deltaTime, Vector2 boundsMin, Vector2 boundsMax)
    {
        enemy.position += Vector3.down * speed * deltaTime;
    }
}
