using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private int damage = 1;

    private bool yaImpacto = false;

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
        if (yaImpacto) return;

        // 1. Lateral del Jefe Final — flash + daño al maestro
        JefeLateral jefeLateral = other.GetComponentInParent<JefeLateral>();
        if (jefeLateral != null)
        {
            yaImpacto = true;
            jefeLateral.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // 2. Jefe Final maestro (centro)
        JefeFinal jefeFinal = other.GetComponentInParent<JefeFinal>();
        if (jefeFinal != null)
        {
            yaImpacto = true;
            jefeFinal.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // 3. Boss principal (Patrulla 1)
        BossHealth bossHealth = other.GetComponentInParent<BossHealth>();
        if (bossHealth != null)
        {
            yaImpacto = true;
            bossHealth.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // 4. Patrulla secundaria (Patrulla 2 y 3)
        PatrullaSecundaria patrullaSecundaria = other.GetComponentInParent<PatrullaSecundaria>();
        if (patrullaSecundaria != null)
        {
            yaImpacto = true;
            patrullaSecundaria.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}