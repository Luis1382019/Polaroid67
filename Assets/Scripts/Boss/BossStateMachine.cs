using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BossStateMachine — Polaroid 67 (v3)
///
/// Las fases ya NO dependen del HP total.
/// Cada patrulla avisa AvanzarFase() al morir.
/// Fase 0 → Fase 1 → Fase 2 → Fase 3 (última)
/// </summary>
public class BossStateMachine : MonoBehaviour
{
    public enum AttackPattern
    {
        SingleShot,
        SpreadShot,
        BurstFire,
        Spiral,
        WaveRain,
        AimedSpread,
        SixSevenPattern,
        FlashFreeze,
        CrossFire,
    }

    [System.Serializable]
    public class PhaseAttack
    {
        public AttackPattern pattern = AttackPattern.SpreadShot;
        [Range(0.1f, 5f)] public float fireRate = 1f;
        [Range(1, 24)] public int bulletCount = 3;
        [Range(5f, 360f)] public float spreadAngle = 45f;
        public float bulletSpeed = 5f;
        [Range(1, 10)] public int repetitions = 2;
    }

    [System.Serializable]
    public class BossPhase
    {
        public string phaseName = "Fase 1";
        public Sprite phaseSprite;
        public float moveSpeed = 2f;
        [Range(0f, 3f)] public float transitionPause = 1f;
        public List<PhaseAttack> attacks = new List<PhaseAttack>();
    }

    [Header("Phases")]
    [SerializeField] private List<BossPhase> phases = new List<BossPhase>();

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform playerTransform;

    [Header("Patrullas")]
    [SerializeField] private PatrullaSpawnSystem patrullaSpawnSystem;

    [Header("Fase 4 — HP de Patrulla 1 al reaparecer")]
    [SerializeField] private int hpFase4 = 20;

    [Header("Visual FX")]
    [SerializeField] private bool useHitFlash = true;
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private float hitFlashTime = 0.08f;
    [SerializeField] private bool usePhaseShake = true;
    [SerializeField] private float shakeIntensity = 0.25f;
    [SerializeField] private float shakeDuration = 0.4f;

    private int currentPhaseIndex = -1;
    private int currentAttackIndex = 0;
    private int currentRepetition = 0;
    private bool isActive = false;
    private bool isTransitioning = false;

    private BossHealth bossHealth;
    private BossVerticalMovement bossMovement;
    private Color originalColor;

    private void Awake()
    {
        bossHealth = GetComponent<BossHealth>();
        bossMovement = GetComponent<BossVerticalMovement>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        isActive = true;
        EnterPhase(0);
        StartCoroutine(AttackLoop());
    }

    // ──────────────────────────────────────────────
    //  API PÚBLICA
    // ──────────────────────────────────────────────

    /// <summary>
    /// Llamado por BossHealth o PatrullaSecundaria al morir.
    /// Avanza automáticamente a la siguiente fase.
    /// </summary>
    public void AvanzarFase()
    {
        Debug.Log("AVANZAR FASE -> actual = " + currentPhaseIndex);

        if (!isActive) return;

        isTransitioning = false;

        int siguiente = currentPhaseIndex + 1;

        Debug.Log("AVANZAR FASE -> siguiente = " + siguiente);

        if (siguiente < phases.Count)
            StartCoroutine(TransitionToPhase(siguiente));
    }
    public void PausarAtaques(bool pausar)
    {
        isTransitioning = pausar;
    }

    public void OnBossDefeated()
    {
        isActive = false;
        StopAllCoroutines();
        if (patrullaSpawnSystem != null)
            patrullaSpawnSystem.LimpiarPatrullas();
    }

    // ──────────────────────────────────────────────
    //  FASES
    // ──────────────────────────────────────────────

    private void EnterPhase(int index)
    {
        if (index < 0 || index >= phases.Count) return;

        currentPhaseIndex = index;
        currentAttackIndex = 0;
        currentRepetition = 0;

        BossPhase phase = phases[index];

        if (spriteRenderer != null && phase.phaseSprite != null)
            spriteRenderer.sprite = phase.phaseSprite;

        if (bossMovement != null)
            bossMovement.SetMoveSpeed(phase.moveSpeed);

        Debug.Log($"[Boss] Entrando a fase {index}: {phase.phaseName}");


    }

    private IEnumerator TransitionToPhase(int index)
    {
        Debug.Log("TRANSITION TO PHASE = " + index);

        isTransitioning = true;

        BossPhase phase = phases[index];

        if (usePhaseShake)
            StartCoroutine(ScreenShake(shakeIntensity, shakeDuration));

        if (phase.transitionPause > 0f)
            yield return new WaitForSeconds(phase.transitionPause);

        EnterPhase(index);

        if (patrullaSpawnSystem != null)
        {
            if (index == 1) patrullaSpawnSystem.IniciarFase2();
            if (index == 2) patrullaSpawnSystem.IniciarFase3();
            if (index == 3)
            {
                // Fase 4: resetear HP de Patrulla 1 y marcarla como última fase
                if (bossHealth != null)
                {
                    bossHealth.ResetHP(hpFase4);
                    bossHealth.SetEsUltimaFase(true);
                }
                patrullaSpawnSystem.IniciarFase4();
            }
        }

        isTransitioning = false;
    }

    // ──────────────────────────────────────────────
    //  LOOP DE ATAQUE (solo Patrulla 1)
    // ──────────────────────────────────────────────

    private IEnumerator AttackLoop()
    {
        yield return new WaitForSeconds(1.5f);

        while (isActive)
        {
            if (isTransitioning)
            {
                yield return null;
                continue;
            }

            // Solo dispara en Fase 0 (Patrulla 1 activa) y Fase 3 (Fase 4)
            bool patrulla1Visible = currentPhaseIndex == 0;
            if (!patrulla1Visible)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            BossPhase phase = phases[currentPhaseIndex];

            if (phase.attacks.Count == 0)
            {
                yield return null;
                continue;
            }

            PhaseAttack attack = phase.attacks[currentAttackIndex];
            yield return StartCoroutine(ExecutePattern(attack));

            if (!isActive) yield break;

            currentRepetition++;
            if (currentRepetition >= attack.repetitions)
            {
                currentRepetition = 0;
                currentAttackIndex = (currentAttackIndex + 1) % phase.attacks.Count;
            }

            yield return new WaitForSeconds(attack.fireRate);
        }
    }

    private IEnumerator ExecutePattern(PhaseAttack attack)
    {
        switch (attack.pattern)
        {
            case AttackPattern.SingleShot:
                yield return StartCoroutine(Pattern_SingleShot(attack)); break;
            case AttackPattern.SpreadShot:
                yield return StartCoroutine(Pattern_SpreadShot(attack)); break;
            case AttackPattern.BurstFire:
                yield return StartCoroutine(Pattern_BurstFire(attack)); break;
            case AttackPattern.Spiral:
                yield return StartCoroutine(Pattern_Spiral(attack)); break;
            case AttackPattern.WaveRain:
                yield return StartCoroutine(Pattern_WaveRain(attack)); break;
            case AttackPattern.AimedSpread:
                yield return StartCoroutine(Pattern_AimedSpread(attack)); break;
            case AttackPattern.SixSevenPattern:
                yield return StartCoroutine(Pattern_SixSeven(attack)); break;
            case AttackPattern.FlashFreeze:
                yield return StartCoroutine(Pattern_FlashFreeze(attack)); break;
            case AttackPattern.CrossFire:
                yield return StartCoroutine(Pattern_CrossFire(attack)); break;
        }
    }

    private IEnumerator Pattern_SingleShot(PhaseAttack attack)
    { SpawnBullet(Vector2.left, attack.bulletSpeed); yield return null; }

    private IEnumerator Pattern_SpreadShot(PhaseAttack attack)
    { SpawnSpread(Vector2.left, attack.bulletCount, attack.spreadAngle, attack.bulletSpeed); yield return null; }

    private IEnumerator Pattern_BurstFire(PhaseAttack attack)
    {
        int burstCount = Mathf.Max(attack.bulletCount, 3);
        for (int i = 0; i < burstCount; i++)
        {
            SpawnBullet(Vector2.left, attack.bulletSpeed);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator Pattern_Spiral(PhaseAttack attack)
    {
        float angleStep = 360f / attack.bulletCount;
        for (int i = 0; i < attack.bulletCount; i++)
            SpawnBullet(AngleToDirection(angleStep * i), attack.bulletSpeed);
        yield return null;
    }

    private IEnumerator Pattern_WaveRain(PhaseAttack attack)
    {
        int waves = Mathf.Max(attack.bulletCount / 2, 2);
        for (int w = 0; w < waves; w++)
        {
            SpawnBulletAtOffset(new Vector2(0f, 2.5f), Vector2.down, attack.bulletSpeed);
            SpawnBulletAtOffset(new Vector2(0f, -2.5f), Vector2.up, attack.bulletSpeed);
            yield return new WaitForSeconds(0.18f);
        }
    }

    private IEnumerator Pattern_AimedSpread(PhaseAttack attack)
    { SpawnSpread(GetDirectionToPlayer(), attack.bulletCount, attack.spreadAngle, attack.bulletSpeed); yield return null; }

    private IEnumerator Pattern_SixSeven(PhaseAttack attack)
    {
        SpawnSpread(Vector2.left, 6, 30f, attack.bulletSpeed);
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(0.67f);
        SpawnSpread(GetDirectionToPlayer(), 7, 50f, attack.bulletSpeed * 1.2f);
    }

    private IEnumerator Pattern_FlashFreeze(PhaseAttack attack)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.15f);
            spriteRenderer.color = originalColor;
        }
        SpawnSpread(Vector2.left, attack.bulletCount, 180f, attack.bulletSpeed * 0.7f);
        yield return null;
    }

    private IEnumerator Pattern_CrossFire(PhaseAttack attack)
    {
        Vector2 aimDir = GetDirectionToPlayer();
        Vector2 perpDir = new Vector2(-aimDir.y, aimDir.x);
        SpawnBullet(aimDir, attack.bulletSpeed);
        SpawnBullet(perpDir, attack.bulletSpeed);
        SpawnBullet(-perpDir, attack.bulletSpeed);
        yield return new WaitForSeconds(0.2f);
        SpawnBullet(aimDir, attack.bulletSpeed * 1.3f);
    }

    private void SpawnBullet(Vector2 direction, float speed)
    {
        Debug.Log(
            "[BossStateMachine] " +
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

    private void SpawnBulletAtOffset(Vector2 offset, Vector2 direction, float speed)
    {
        if (bulletPrefab == null || firePoint == null) return;
        Vector3 spawnPos = firePoint.position + (Vector3)offset;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        GameObject go = Instantiate(bulletPrefab, spawnPos, Quaternion.Euler(0, 0, angle));
        BossBullet b = go.GetComponent<BossBullet>();
        if (b != null) b.SetSpeed(speed);
    }

    private void SpawnSpread(Vector2 baseDirection, int count, float spread, float speed)
    {
        if (count <= 1) { SpawnBullet(baseDirection, speed); return; }
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
        float startAngle = baseAngle - spread / 2f;
        float step = spread / (count - 1);
        for (int i = 0; i < count; i++)
            SpawnBullet(AngleToDirection(startAngle + step * i), speed);
    }

    private Vector2 GetDirectionToPlayer()
    {
        if (playerTransform == null) return Vector2.left;
        return ((Vector2)(playerTransform.position - transform.position)).normalized;
    }

    private Vector2 AngleToDirection(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    public IEnumerator HitFlash()
    {
        if (!useHitFlash || spriteRenderer == null) yield break;
        spriteRenderer.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashTime);
        spriteRenderer.color = originalColor;
    }

    private IEnumerator ScreenShake(float intensity, float duration)
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;
        Vector3 originalPos = cam.transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            cam.transform.position = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cam.transform.position = originalPos;
    }
}