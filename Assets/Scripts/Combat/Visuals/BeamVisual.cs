using UnityEngine;

public class BeamVisual : MonoBehaviour, IPoolable
{
    [SerializeField] private float lifetime = 0.08f;
    private float timer;

    private void OnEnable()
    {
        timer = lifetime;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.Despawn(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    public void OnSpawn() { }
    public void OnDespawn() { }
}
