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
        Debug.Log("Bala tocó: " + other.name);

        BossHealth bossHealth = other.GetComponentInParent<BossHealth>();

        if (bossHealth != null)
        {
            Debug.Log("Bala golpeó al boss");
            bossHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}