using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 20;

    [Header("Boss Systems")]
    [SerializeField] private BossCarController bossCarController;
    [SerializeField] private BossVerticalMovement bossMovement;

    [Header("Win")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private string nextSceneName = "ChapterSelect";
    [SerializeField] private float delayBeforeLeaving = 2f;

    private int currentHealth;
    private bool isDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        Debug.Log("Vida del boss: " + currentHealth);

        if (currentHealth <= 0)
        {
            StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        isDead = true;

        Debug.Log("Boss derrotado");

        if (bossCarController != null)
        {
            bossCarController.StopBoss();
        }

        if (bossMovement != null)
        {
            bossMovement.enabled = false;
        }

        BossBullet[] bullets = Object.FindObjectsByType<BossBullet>(
            FindObjectsInactive.Exclude
        );

        foreach (BossBullet bullet in bullets)
        {
            Destroy(bullet.gameObject);
        }

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        yield return new WaitForSeconds(delayBeforeLeaving);

        SceneManager.LoadScene(nextSceneName);
    }
}