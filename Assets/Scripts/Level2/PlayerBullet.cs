using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private int damage = 1;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += transform.right * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Jefe Final (Fase 4 — HP compartido)
        JefeFinal jefeFinal = other.GetComponentInParent<JefeFinal>();
        if (jefeFinal != null)
        {
            jefeFinal.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // 2. Boss principal (Patrulla 1, Fases 1 y 4)
        BossHealth bossHealth = other.GetComponentInParent<BossHealth>();
        if (bossHealth != null)
        {
            bossHealth.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // 3. Patrulla secundaria (Patrulla 2 y 3, Fases 2 y 3)
        PatrullaSecundaria patrullaSecundaria = other.GetComponentInParent<PatrullaSecundaria>();
        if (patrullaSecundaria != null)
        {
            patrullaSecundaria.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}