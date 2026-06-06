using System.Collections;
using UnityEngine;

public class DamageCarObstacle : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private Vector2 moveDirection = Vector2.left;
    [SerializeField] private float lifeTime = 8f;

    [Header("Entrada natural")]
    [Tooltip("Cuántas unidades a la derecha del spawn aparece antes de entrar")]
    [SerializeField] private float entradaOffset = 6f;
    [Tooltip("Velocidad a la que entra hasta su posición de carril")]
    [SerializeField] private float velocidadEntrada = 10f;

    [Header("Damage To Player")]
    [SerializeField] private int damage = 1;

    [Header("Health")]
    [SerializeField] private int maxHealth = 1;

    private int currentHealth;
    private bool isMoving = false;
    private bool isDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
        StartCoroutine(EntradaNatural());
    }

    private IEnumerator EntradaNatural()
    {
        // Posición de carril destino (donde spawnea normalmente)
        Vector3 destino = transform.position;

        // Aparecer más a la derecha
        transform.position = destino + Vector3.right * entradaOffset;

        // Moverse hasta el carril
        while (Vector3.Distance(transform.position, destino) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                destino,
                velocidadEntrada * Time.deltaTime
            );
            yield return null;
        }

        transform.position = destino;

        // Ya en posición, empieza a moverse normalmente
        isMoving = true;
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
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        isDead = true;
        RoadKillObjective objective = Object.FindAnyObjectByType<RoadKillObjective>();
        if (objective != null) objective.AddKill();
        Destroy(gameObject);
    }

    public void SetSpeed(float newSpeed) { moveSpeed = newSpeed; }
    public float GetSpeed() { return moveSpeed; }
    public void StopMovement() { isMoving = false; }
}