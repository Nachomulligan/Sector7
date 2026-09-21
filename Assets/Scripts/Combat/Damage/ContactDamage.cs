using UnityEngine;

[RequireComponent(typeof(Health))]
public class ContactDamage : MonoBehaviour
{
    [SerializeField] private int damage = 15;
    [Tooltip("Si está activo, este objeto también se destruye a sí mismo al golpear (kamikaze).")]
    [SerializeField] private bool destroySelfOnHit = false;

    private Health ownHealth;

    private void Awake()
    {
        ownHealth = GetComponent<Health>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out IDamageable damageable) || damageable.Faction == ownHealth.Faction)
        {
            return;
        }

        damageable.TakeDamage(damage, gameObject);

        if (destroySelfOnHit)
        {
            ownHealth.TakeDamage(ownHealth.MaxHealth, other.gameObject);
        }
    }
}
