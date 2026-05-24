using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;

    [Header("Game Over")]
    [SerializeField] private LoopingNewRoad roadLoop;
    [SerializeField] private RandomCarSpawner carSpawner;
    [SerializeField] private BossCarController bossController;
    [SerializeField] private BossVerticalMovement bossMovement;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Exit")]
    [SerializeField] private string sceneToLoadOnDeath = "ChapterSelect";
    [SerializeField] private float delayBeforeExit = 2f;

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

        Debug.Log("GAME OVER");

        if (roadLoop != null)
        {
            roadLoop.StopLoop();
        }

        if (carSpawner != null)
        {
            carSpawner.enabled = false;
        }

        if (bossController != null)
        {
            bossController.StopBoss();
        }

        if (bossMovement != null)
        {
            bossMovement.enabled = false;
        }

        DamageCarObstacle[] cars = Object.FindObjectsByType<DamageCarObstacle>(
            FindObjectsInactive.Exclude
        );

        foreach (DamageCarObstacle car in cars)
        {
            car.StopMovement();
        }

        BossBullet[] bossBullets = Object.FindObjectsByType<BossBullet>(
            FindObjectsInactive.Exclude
        );

        foreach (BossBullet bullet in bossBullets)
        {
            Destroy(bullet.gameObject);
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

        StartCoroutine(ExitAfterDelay());
    }

    private IEnumerator ExitAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeExit);

        SceneManager.LoadScene(sceneToLoadOnDeath);
    }
}