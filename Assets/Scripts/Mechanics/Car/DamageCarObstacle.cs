using UnityEngine;

public class DamageCarObstacle : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private Vector2 moveDirection = Vector2.left;
    [SerializeField] private float lifeTime = 8f;

    [Header("Damage To Player")]
    [SerializeField] private int damage = 1;

    [Header("Health")]
    [SerializeField] private int maxHealth = 1;

    private int currentHealth;
    private bool isMoving = true;
    private bool isDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (!isMoving) return;

        transform.position += (Vector3)(moveDirection.normalized * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        RoadKillObjective objective = Object.FindAnyObjectByType<RoadKillObjective>();

        if (objective != null)
        {
            objective.AddKill();
        }

        Destroy(gameObject);
    }

    public void SetSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    public float GetSpeed()
    {
        return moveSpeed;
    }

    public void StopMovement()
    {
        isMoving = false;
    }
}