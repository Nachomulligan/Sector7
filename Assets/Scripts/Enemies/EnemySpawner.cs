using UnityEngine;
public class EnemySpawner : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private float spawnY = 6f;
    [SerializeField] private float minX = -3.5f;
    [SerializeField] private float maxX = 3.5f;

    private float timer;

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnEnemy();
            timer = spawnInterval;
        }
    }

    private void SpawnEnemy()
    {
        float x = Random.Range(minX, maxX);
        Vector3 position = new Vector3(x, spawnY, 0f);

        PoolManager.Instance.Spawn(enemyPrefab, position, Quaternion.identity);
    }
}
