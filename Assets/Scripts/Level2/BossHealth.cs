using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// BossHealth — Polaroid 67 (v4)
///
/// Ahora cada patrulla tiene su propio HP.
/// Al llegar a 0, avisa a BossStateMachine para avanzar de fase.
/// La Patrulla 1 usa este script. Patrullas 2 y 3 usan PatrullaSecundaria.
/// </summary>
public class BossHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 30;

    [Header("Boss Systems")]
    [SerializeField] private BossStateMachine bossStateMachine;
    [SerializeField] private BossVerticalMovement bossMovement;

    [Header("HP Bar (opcional)")]
    [SerializeField] private UnityEngine.UI.Slider hpSlider;

    [Header("Win (solo Fase 4)")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private string nextSceneName = "ChapterSelect";
    [SerializeField] private float delayBeforeLeaving = 2f;

    [Tooltip("Marcar solo en la Fase 4 — al morir muestra el panel de victoria")]
    [SerializeField] private bool esUltimaFase = false;

    private int currentHealth;
    private bool isDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (winPanel != null) winPanel.SetActive(false);
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = maxHealth;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (hpSlider != null)
            hpSlider.value = currentHealth;

        if (bossStateMachine != null)
            StartCoroutine(bossStateMachine.HitFlash());

        Debug.Log($"[BossHealth] HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            StartCoroutine(Die());
    }

    private IEnumerator Die()
    {
        isDead = true;
        Debug.Log("[BossHealth] Patrulla 1 derrotada — avanzando fase");

        if (bossMovement != null)
            bossMovement.enabled = false;

        // Ocultar slider al morir — via CanvasGroup
        if (hpSlider != null)
        {
            CanvasGroup cg = hpSlider.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 0f;
        }

        // Limpiar balas
        BossBullet[] bullets = Object.FindObjectsByType<BossBullet>(FindObjectsInactive.Exclude);
        foreach (BossBullet bullet in bullets)
            Destroy(bullet.gameObject);

        if (esUltimaFase)
        {
            // Fase 4 terminada — victoria
            if (bossStateMachine != null)
                bossStateMachine.OnBossDefeated();

            if (winPanel != null)
                winPanel.SetActive(true);

            yield return new WaitForSeconds(delayBeforeLeaving);
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.Log("LLAMANDO AVANZAR FASE");

            if (bossStateMachine != null)
                bossStateMachine.AvanzarFase();
        }
    }

    public float GetHealthRatio() => (float)currentHealth / maxHealth;

    // Permite resetear el HP cuando la Patrulla 1 reaparece en Fase 4
    public void ResetHP(int nuevoMaxHP)
    {
        maxHealth = nuevoMaxHP;
        currentHealth = nuevoMaxHP;
        isDead = false;

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = maxHealth;
        }
    }

    public void SetEsUltimaFase(bool valor)
    {
        esUltimaFase = valor;
    }

    public void SetSlider(Slider slider)
    {
        hpSlider = slider;

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }
    }
}