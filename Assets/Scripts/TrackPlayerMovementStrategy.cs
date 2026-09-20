using UnityEngine;

[CreateAssetMenu(menuName = "Sector7/Movement/Track Player", fileName = "Movement_TrackPlayer")]
public class TrackPlayerMovementStrategy : MovementStrategySO
{
    [SerializeField] private float verticalSpeed = 2.5f;
    [SerializeField] private float horizontalTrackSpeed = 2f;

    public override void Move(Transform enemy, MovementState state, float deltaTime, Vector2 boundsMin, Vector2 boundsMax)
    {
        Vector3 pos = enemy.position;
        pos.y -= verticalSpeed * deltaTime;

        Transform player = FindPlayer();

        if (player != null)
        {
            pos.x = Mathf.MoveTowards(pos.x, player.position.x, horizontalTrackSpeed * deltaTime);
        }

        enemy.position = pos;
    }

    private Transform FindPlayer()
    {
        Health[] allHealth = Object.FindObjectsOfType<Health>();

        foreach (Health health in allHealth)
        {
            if (health.Faction == Faction.Player && health.IsAlive)
            {
                return health.transform;
            }
        }

        return null;
    }
}
