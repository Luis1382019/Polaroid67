using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PatrullaSecundaria — Polaroid 67 (v5)
///
/// Cambios:
///   - Patrones de ataque configurables en el Inspector (igual que BossStateMachine).
///   - Al morir, sale hacia la derecha (marcha atrás) antes de destruirse.
///   - SpawnDerecha se usa solo como referencia de X para calcular el destino de salida.
/// </summary>
public class PatrullaSecundaria : MonoBehaviour
{
    public enum AttackPattern
    {
        SpreadShot,     // Abanico apuntado al jugador
        BurstFire,      // Ráfaga rápida
        SingleShot,     // Un disparo directo
        CrossFire,      // Directo + perpendicular
        Spiral,         // Espiral en todas direcciones
    }

    [System.Serializable]
    public class PatrullaAtaque
    {
        public AttackPattern pattern = AttackPattern.SpreadShot;
        [Range(0.5f, 5f)] public float fireRate = 1.2f;
        [Range(1, 12)] public int bulletCount = 3;
        [Range(5f, 180f)] public float spreadAngle = 40f;
        public float bulletSpeed = 6f;
        [Range(1, 10)] public int repetitions = 2;
    }

    [Header("HP")]
    [SerializeField] private int maxHealth = 25;

    [Header("Patrones de Ataque")]
    [SerializeField] private List<PatrullaAtaque> ataques = new List<PatrullaAtaque>();

    [Header("Referencias")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform spawnDerecha; // Solo referencia de X para salida

    [Tooltip("Cuántas unidades más allá del SpawnDerecha sale la patrulla al morir")]
    [SerializeField] private float xSalidaOffset = 3f;
    [SerializeField] private float velocidadSalida = 7f;

    [Header("UI")]
    [SerializeField] private UnityEngine.UI.Slider hpSlider;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private float hitFlashTime = 0.08f;

    // ──────────────────────────────────────────────
    //  ESTADO INTERNO
    // ──────────────────────────────────────────────

    private Color originalColor;
    private int currentHealth;
    private bool isDead = false;
    private bool isActive = false;
    private int currentAtaqueIndex = 0;
    private int currentRepetition = 0;
    private Transform playerTransform;
    private BossStateMachine bossStateMachine;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        currentHealth = maxHealth;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        bossStateMachine = Object.FindAnyObjectByType<BossStateMachine>();

        if (firePoint == null) firePoint = transform;

        // Buscar SpawnDerecha en escena si no está asignado
        if (spawnDerecha == null)
        {
            GameObject sd = GameObject.Find("SpawnDerecha");
            if (sd != null) spawnDerecha = sd.transform;
        }
    }

    // ──────────────────────────────────────────────
    //  API PÚBLICA
    // ──────────────────────────────────────────────

    public void Activar()
    {
        isActive = true;
        isDead = false;
        currentHealth = maxHealth;
        currentAtaqueIndex = 0;
        currentRepetition = 0;

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = maxHealth;
        }

        StopAllCoroutines();
        StartCoroutine(AttackLoop());

        // Solo activar movimiento si no es parte del Jefe Final (que debe estar estático)
        bool esJefeFinal = GetComponent<JefeFinal>() != null;
        BossVerticalMovement mov = GetComponent<BossVerticalMovement>();
        if (mov != null && !esJefeFinal) mov.enabled = true;
    }

    public void PausarAtaques(bool pausar)
    {
        isActive = !pausar;
    }

    public void Detener()
    {
        isActive = false;
        StopAllCoroutines();
    }

    // ──────────────────────────────────────────────
    //  DAÑO
    // ──────────────────────────────────────────────

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (hpSlider != null)
            hpSlider.value = currentHealth;

        if (currentHealth <= 0)
            Morir();
        else
            StartCoroutine(HitFlash());
    }

    private void Morir()
    {
        if (isDead) return;
        isDead = true;
        isActive = false;

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        StopAllCoroutines();

        // Ocultar slider al morir — via CanvasGroup para no romper referencias
        if (hpSlider != null)
        {
            CanvasGroup cg = hpSlider.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 0f;
        }

        // Limpiar balas
        BossBullet[] bullets = Object.FindObjectsByType<BossBullet>(FindObjectsInactive.Exclude);
        foreach (BossBullet bullet in bullets)
            Destroy(bullet.gameObject);

        // Avisar a BossStateMachine
        if (bossStateMachine != null)
            bossStateMachine.AvanzarFase();

        // Salir hacia la derecha antes de destruirse
        StartCoroutine(SalirYDestruir());
    }

    private IEnumerator SalirYDestruir()
    {
        // Desactivar collider para que no reciba más daño mientras sale
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Calcular destino: X del SpawnDerecha + offset, misma Y actual
        float destinoX = spawnDerecha != null
            ? spawnDerecha.position.x + xSalidaOffset
            : transform.position.x + 15f;

        Vector3 destino = new Vector3(destinoX, transform.position.y, transform.position.z);

        // Mover hacia la derecha
        while (Vector3.Distance(transform.position, destino) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                destino,
                velocidadSalida * Time.deltaTime
            );
            yield return null;
        }

        Destroy(gameObject);
    }

    // ──────────────────────────────────────────────
    //  LOOP DE ATAQUE
    // ──────────────────────────────────────────────

    private IEnumerator AttackLoop()
    {
        yield return new WaitForSeconds(0.8f);

        while (isActive)
        {
            if (ataques.Count == 0)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            PatrullaAtaque ataque = ataques[currentAtaqueIndex];
            yield return StartCoroutine(EjecutarPatron(ataque));

            currentRepetition++;
            if (currentRepetition >= ataque.repetitions)
            {
                currentRepetition = 0;
                currentAtaqueIndex = (currentAtaqueIndex + 1) % ataques.Count;
            }

            yield return new WaitForSeconds(ataque.fireRate);
        }
    }

    private IEnumerator EjecutarPatron(PatrullaAtaque ataque)
    {
        switch (ataque.pattern)
        {
            case AttackPattern.SpreadShot:
                SpawnSpread(GetDirJugador(), ataque.bulletCount, ataque.spreadAngle, ataque.bulletSpeed);
                break;

            case AttackPattern.SingleShot:
                SpawnBullet(GetDirJugador(), ataque.bulletSpeed);
                break;

            case AttackPattern.BurstFire:
                for (int i = 0; i < Mathf.Max(ataque.bulletCount, 3); i++)
                {
                    SpawnBullet(GetDirJugador(), ataque.bulletSpeed);
                    yield return new WaitForSeconds(0.12f);
                }
                break;

            case AttackPattern.CrossFire:
                Vector2 aim = GetDirJugador();
                Vector2 perp = new Vector2(-aim.y, aim.x);
                SpawnBullet(aim, ataque.bulletSpeed);
                SpawnBullet(perp, ataque.bulletSpeed);
                SpawnBullet(-perp, ataque.bulletSpeed);
                yield return new WaitForSeconds(0.2f);
                SpawnBullet(aim, ataque.bulletSpeed * 1.3f);
                break;

            case AttackPattern.Spiral:
                float step = 360f / Mathf.Max(ataque.bulletCount, 4);
                for (int i = 0; i < ataque.bulletCount; i++)
                {
                    float rad = (step * i) * Mathf.Deg2Rad;
                    SpawnBullet(new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)), ataque.bulletSpeed);
                }
                break;
        }
        yield return null;
    }

    // ──────────────────────────────────────────────
    //  HELPERS
    // ──────────────────────────────────────────────

    private Vector2 GetDirJugador()
    {
        if (playerTransform == null) return Vector2.left;
        return ((Vector2)(playerTransform.position - transform.position)).normalized;
    }

    private void SpawnBullet(Vector2 direction, float speed)
    {
        Debug.Log(
            "[PatrullaSecundaria] " +
            gameObject.name +
            " dispara desde " +
            firePoint.position
        );

        if (bulletPrefab == null || firePoint == null) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject go = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.Euler(0, 0, angle)
        );

        BossBullet b = go.GetComponent<BossBullet>();

        if (b != null)
            b.SetSpeed(speed);
    }
    private void SpawnSpread(Vector2 baseDir, int count, float spread, float speed)
    {
        if (count <= 1) { SpawnBullet(baseDir, speed); return; }
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
        float startAngle = baseAngle - spread / 2f;
        float stepAngle = spread / (count - 1);
        for (int i = 0; i < count; i++)
        {
            float a = (startAngle + stepAngle * i) * Mathf.Deg2Rad;
            SpawnBullet(new Vector2(Mathf.Cos(a), Mathf.Sin(a)), speed);
        }
    }

    public void SetSlider(UnityEngine.UI.Slider slider)
    {
        hpSlider = slider;
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }
    }

    private IEnumerator HitFlash()
    {
        if (spriteRenderer == null) yield break;
        spriteRenderer.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashTime);
        spriteRenderer.color = originalColor;
    }

    public void DisparoAbanico(
    int cantidadBalas,
    float angulo,
    float velocidad)
    {
        SpawnSpread(
            GetDirJugador(),
            cantidadBalas,
            angulo,
            velocidad
        );
    }

    /// <summary>
    /// Dispara abanico en dirección fija (no apunta al jugador).
    /// Útil para cubrir carriles sin seguir al jugador.
    /// direccion: Vector2.left para disparar recto a la izquierda.
    /// </summary>
    public void DisparoAbanicoFijo(
        int cantidadBalas,
        float angulo,
        float velocidad,
        Vector2 direccion)
    {
        SpawnSpread(direccion.normalized, cantidadBalas, angulo, velocidad);
    }

    public void DisparoRafaga(
        int cantidadBalas,
        float velocidad)
    {
        StartCoroutine(Rafaga(cantidadBalas, velocidad));
    }

    private IEnumerator Rafaga(
        int cantidadBalas,
        float velocidad)
    {
        for (int i = 0; i < cantidadBalas; i++)
        {
            SpawnBullet(
                GetDirJugador(),
                velocidad
            );

            yield return new WaitForSeconds(0.1f);
        }
    }

}