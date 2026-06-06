using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JefeFinal : MonoBehaviour
{
    [Header("HP Compartido")]
    [Tooltip("Solo marcar en UNA de las 3 patrullas (la del centro)")]
    [SerializeField] private bool isMaster = false;
    [SerializeField] private int maxHealth = 20;

    [Header("Win (solo en el maestro)")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private string nextSceneName = "ChapterSelect";
    [SerializeField] private float delayBeforeLeaving = 2f;

    [Header("UI")]
    [SerializeField] private UnityEngine.UI.Slider hpSlider;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private float hitFlashTime = 0.08f;

    private static int sharedHealth;
    private static bool jefeMuerto = false;
    private Color originalColor;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        if (isMaster)
        {
            sharedHealth = maxHealth;
            jefeMuerto = false;
            Debug.Log($"[JefeFinal Start] sharedHealth={sharedHealth} hpSlider={hpSlider}");
            if (hpSlider != null)
                Debug.Log($"[JefeFinal Start] slider.value={hpSlider.value}");

    }

        BossVerticalMovement mov = GetComponent<BossVerticalMovement>();
        if (mov != null) mov.enabled = false;

        PatrullaSecundaria ps = GetComponent<PatrullaSecundaria>();
        if (ps != null && !isMaster) ps.PausarAtaques(true);
    }

    public void SetSlider(UnityEngine.UI.Slider slider)
    {
        hpSlider = slider;
        if (hpSlider != null && isMaster)
        {
            // Resetear sharedHealth siempre al asignar slider
            sharedHealth = maxHealth;
            jefeMuerto = false;
            hpSlider.maxValue = maxHealth;
            hpSlider.value = maxHealth;
            Debug.Log($"[JefeFinal] Slider asignado — HP reseteado a {maxHealth}");
        }
    }

    public void TakeDamage(int damage)
    {
        if (jefeMuerto) return;

        sharedHealth -= damage;
        sharedHealth = Mathf.Max(sharedHealth, 0);

        Debug.Log($"[JefeFinal] HP: {sharedHealth}/{maxHealth}");

        if (hpSlider != null) hpSlider.value = sharedHealth;

        JefeFinal[] todos = Object.FindObjectsByType<JefeFinal>(FindObjectsInactive.Exclude);
        foreach (JefeFinal jefe in todos)
            jefe.StartCoroutine(jefe.HitFlash());

        if (sharedHealth <= 0 && isMaster)
            StartCoroutine(Morir());
    }

    private IEnumerator Morir()
    {
        jefeMuerto = true;
        Debug.Log("[JefeFinal] Victoria!");

        if (hpSlider != null)
        {
            CanvasGroup cg = hpSlider.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 0f;
        }

        BossBullet[] bullets = Object.FindObjectsByType<BossBullet>(FindObjectsInactive.Exclude);
        foreach (BossBullet bullet in bullets)
            Destroy(bullet.gameObject);

        BossStateMachine bsm = Object.FindAnyObjectByType<BossStateMachine>();
        if (bsm != null) bsm.OnBossDefeated();

        if (winPanel != null) winPanel.SetActive(true);

        yield return new WaitForSeconds(delayBeforeLeaving);
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator HitFlash()
    {
        if (spriteRenderer == null) yield break;
        spriteRenderer.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashTime);
        spriteRenderer.color = originalColor;
    }


}