using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// JefeFinal — Polaroid 67
///
/// HP compartido entre las 3 patrullas de la Fase 4.
/// Agrega este script a los 3 prefabs en Fase 4 (o crea prefabs nuevos).
/// Asigna el mismo valor de maxHealth en los 3 — pero solo uno es "maestro".
///
/// CÓMO FUNCIONA:
///   Al recibir daño, busca a los otros dos JefeFinal en escena y les notifica.
///   Solo el "maestro" (isMaster = true) controla la muerte y el win.
/// </summary>
public class JefeFinal : MonoBehaviour
{
    [Header("HP Compartido")]
    [Tooltip("Solo marcar en UNA de las 3 patrullas (la del centro / Patrulla 2)")]
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

    // HP compartido entre todas las instancias — static
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
        // Solo el maestro inicializa el HP compartido
        if (isMaster)
        {
            sharedHealth = maxHealth;
            jefeMuerto = false;
            Debug.Log($"[JefeFinal] HP compartido inicializado: {sharedHealth}");
            if (hpSlider != null) { hpSlider.maxValue = maxHealth; hpSlider.value = maxHealth; }
        }

        // Desactivar movimiento vertical — estáticas en Fase 4
        BossVerticalMovement mov = GetComponent<BossVerticalMovement>();
        if (mov != null) mov.enabled = false;

        // Desactivar ataques en las no-maestro
        PatrullaSecundaria ps = GetComponent<PatrullaSecundaria>();
        if (ps != null && !isMaster)
            ps.PausarAtaques(true);
    }

    public void TakeDamage(int damage)
    {
        if (jefeMuerto) return;

        sharedHealth -= damage;
        sharedHealth = Mathf.Max(sharedHealth, 0);

        Debug.Log($"[JefeFinal] HP compartido: {sharedHealth}/{maxHealth}");

        // Actualizar slider
        if (hpSlider != null) hpSlider.value = sharedHealth;

        // Flash en TODAS las patrullas del jefe final
        JefeFinal[] todos = Object.FindObjectsByType<JefeFinal>(FindObjectsInactive.Exclude);
        foreach (JefeFinal jefe in todos)
            jefe.StartCoroutine(jefe.HitFlash());

        if (sharedHealth <= 0 && isMaster)
            StartCoroutine(Morir());
    }

    private IEnumerator Morir()
    {
        jefeMuerto = true;
        Debug.Log("[JefeFinal] Jefe Final derrotado — Victoria!");

        // Limpiar balas
        BossBullet[] bullets = Object.FindObjectsByType<BossBullet>(FindObjectsInactive.Exclude);
        foreach (BossBullet bullet in bullets)
            Destroy(bullet.gameObject);

        // Notificar al BossStateMachine
        BossStateMachine bsm = Object.FindAnyObjectByType<BossStateMachine>();
        if (bsm != null) bsm.OnBossDefeated();

        if (winPanel != null)
            winPanel.SetActive(true);

        yield return new WaitForSeconds(delayBeforeLeaving);
        SceneManager.LoadScene(nextSceneName);
    }

    public void SetSlider(UnityEngine.UI.Slider slider)
    {
        hpSlider = slider;
        if (hpSlider != null && isMaster)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = sharedHealth;
        }
    }

    private IEnumerator HitFlash()
    {
        if (spriteRenderer == null) yield break;
        spriteRenderer.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashTime);
        spriteRenderer.color = originalColor;
    }
}