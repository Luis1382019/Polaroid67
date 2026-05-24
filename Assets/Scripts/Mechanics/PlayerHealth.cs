using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;

    [Header("Game Over")]
    [SerializeField] private LoopingNewRoad roadLoop;
    [SerializeField] private RandomCarSpawner carSpawner;
    [SerializeField] private GameObject gameOverPanel;

    private int currentHealth;
    private bool isDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        Debug.Log("Vida actual: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        Debug.Log("Game Over");

        if (roadLoop != null)
        {
            roadLoop.StopLoop();
        }

        if (carSpawner != null)
        {
            carSpawner.enabled = false;
        }

        DamageCarObstacle[] cars = Object.FindObjectsByType<DamageCarObstacle>(FindObjectsInactive.Exclude);

        foreach (DamageCarObstacle car in cars)
        {
            car.enabled = false;
        }

        PlayerMovement playerMovement = GetComponent<PlayerMovement>();

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
}