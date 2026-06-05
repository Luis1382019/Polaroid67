using System.Collections;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// PatrullaSpawnSystem — Polaroid 67 (v6)
///
/// - Patrulla 1 sale hacia la derecha (marcha atrás) en Fase 2 y 3.
/// - Patrullas 2/3 también salen hacia la derecha manteniendo su Y.
/// - Fase 4: Patrulla 1 hace fade out (desvanecimiento) antes de reaparecer arriba.
/// - Spawn siempre usa la Y de la posición de combate, nunca la Y del spawnPoint.
/// </summary>
public class PatrullaSpawnSystem : MonoBehaviour
{
    [Header("Prefabs — Fases 2 y 3")]
    [SerializeField] private GameObject patrulla2Prefab;
    [SerializeField] private GameObject patrulla3Prefab;

    [Header("Prefabs — Fase 4 (Jefe Final)")]
    [Tooltip("Prefab del centro — tiene JefeFinal con isMaster=true y PatrullaSecundaria")]
    [SerializeField] private GameObject jefeFinalCentroPrefab;
    [Tooltip("Prefab lateral izquierdo — tiene JefeFinal con isMaster=false, sin ataques")]
    [SerializeField] private GameObject jefeFinalIzquierdaPrefab;
    [Tooltip("Prefab lateral derecho — tiene JefeFinal con isMaster=false, sin ataques")]
    [SerializeField] private GameObject jefeFinalDerechaPrefab;

    [Header("Puntos de Spawn (solo entrada)")]
    [Tooltip("Fuera de pantalla a la derecha")]
    [SerializeField] private Transform spawnPointDerecha;
    [Tooltip("Fuera de pantalla arriba — entrada en fila Fase 4")]
    [SerializeField] private Transform spawnPointArriba;

    [Header("Posiciones de combate en pantalla")]
    [SerializeField] private Transform posicionPatrulla1;
    [SerializeField] private Transform posicionPatrulla2;
    [SerializeField] private Transform posicionPatrulla3;

    [Header("Fase 4 — Fila desde arriba")]
    [SerializeField] private float separacionFila = 2.5f;
    [SerializeField] private float retardoFila = 0.4f;

    [Header("Movimiento")]
    [SerializeField] private float velocidadEntrada = 5f;
    [SerializeField] private float velocidadSalida = 6f;

    [Header("UI")]
    [SerializeField] private Slider hpSlider;

    // ──────────────────────────────────────────────
    //  ESTADO INTERNO
    // ──────────────────────────────────────────────

    private GameObject patrulla2Instancia;
    private GameObject patrulla3Instancia;
    private GameObject patrulla1 => gameObject;

    private BossStateMachine bossStateMachine;
    private BossVerticalMovement bossMovimiento;
    private SpriteRenderer bossRenderer;
    private Collider2D bossCollider;

    private void Awake()
    {
        bossStateMachine = GetComponent<BossStateMachine>();
        bossMovimiento = GetComponent<BossVerticalMovement>();
        bossRenderer = GetComponent<SpriteRenderer>();
        bossCollider = GetComponent<Collider2D>();
    }

    // ──────────────────────────────────────────────
    //  API PÚBLICA
    // ──────────────────────────────────────────────

    public void IniciarFase2() { StartCoroutine(TransicionFase2()); }
    public void IniciarFase3() { StartCoroutine(TransicionFase3()); }
    public void IniciarFase4() { StartCoroutine(TransicionFase4()); }

    public void LimpiarPatrullas()
    {
        if (patrulla2Instancia != null) Destroy(patrulla2Instancia);
        if (patrulla3Instancia != null) Destroy(patrulla3Instancia);
    }

    // ──────────────────────────────────────────────
    //  OCULTAR / MOSTRAR PATRULLA 1
    // ──────────────────────────────────────────────

    private void OcultarPatrulla1()
    {
        if (bossRenderer != null) bossRenderer.enabled = false;
        if (bossCollider != null) bossCollider.enabled = false;
        if (bossMovimiento != null) bossMovimiento.enabled = false;
        if (bossStateMachine != null) bossStateMachine.PausarAtaques(true);
    }

    private void MostrarPatrulla1()
    {
        if (bossRenderer != null) bossRenderer.enabled = true;
        if (bossCollider != null) bossCollider.enabled = true;
        if (bossMovimiento != null) bossMovimiento.enabled = true;
        if (bossStateMachine != null) bossStateMachine.PausarAtaques(false);
    }

    // ──────────────────────────────────────────────
    //  TRANSICIONES
    // ──────────────────────────────────────────────

    private IEnumerator TransicionFase2()
    {
        // Patrulla 1 para ataques y movimiento, luego sale hacia la derecha (marcha atrás)
        if (bossStateMachine != null) bossStateMachine.PausarAtaques(true);
        // Resetear slider para la siguiente patrulla
        if (hpSlider != null) { hpSlider.maxValue = 25; hpSlider.value = 25; }
        if (bossMovimiento != null) bossMovimiento.enabled = false;

        // Desactivar collider para que no reciba daño mientras sale
        if (bossCollider != null) bossCollider.enabled = false;

        // Sale hacia la derecha manteniendo su Y actual (marcha atrás)
        Vector3 salidaP1 = new Vector3(
            spawnPointDerecha.position.x + 3f,
            patrulla1.transform.position.y,
            patrulla1.transform.position.z
        );
        yield return StartCoroutine(MoverAposicion(patrulla1, salidaP1, velocidadSalida));
        OcultarPatrulla1();

        yield return new WaitForSeconds(0.1f);

        // Patrulla 2 entra desde la derecha con la Y correcta
        if (patrulla2Prefab != null && spawnPointDerecha != null)
        {
            Vector3 spawnPos = new Vector3(
                spawnPointDerecha.position.x,
                posicionPatrulla2.position.y,  // Y de combate, no la del spawnPoint
                spawnPointDerecha.position.z
            );
            patrulla2Instancia = Instantiate(patrulla2Prefab, spawnPos, Quaternion.identity);
            BossHealth bh = patrulla2Instancia.GetComponent<BossHealth>();

            if (bh != null)
            {
                bh.SetSlider(hpSlider);
            }
            DesactivarComponentes(patrulla2Instancia);
            AsignarSlider(patrulla2Instancia);
            yield return StartCoroutine(MoverAposicion(patrulla2Instancia, posicionPatrulla2.position, velocidadEntrada));
            ActivarPatrullaSecundaria(patrulla2Instancia);
        }
    }

    private IEnumerator TransicionFase3()
    {
        // Patrulla 2 comienza a salir
        if (patrulla2Instancia != null)
        {
            DesactivarComponentes(patrulla2Instancia);

            Vector3 salidaP2 = new Vector3(
                spawnPointDerecha.position.x,
                patrulla2Instancia.transform.position.y,
                patrulla2Instancia.transform.position.z
            );

            StartCoroutine(
                MoverAposicion(
                    patrulla2Instancia,
                    salidaP2,
                    velocidadSalida
                )
            );
        }

        // Espera mínima
        yield return new WaitForSeconds(0.25f);

        // Patrulla 3 entra
        if (patrulla3Prefab != null && spawnPointDerecha != null)
        {
            Vector3 spawnPos = new Vector3(
                spawnPointDerecha.position.x,
                posicionPatrulla3.position.y,
                spawnPointDerecha.position.z
            );

            patrulla3Instancia = Instantiate(
                patrulla3Prefab,
                spawnPos,
                Quaternion.identity
            );
            BossHealth bh = patrulla3Instancia.GetComponent<BossHealth>();

            if (bh != null)
            {
                bh.SetSlider(hpSlider);
            }
            DesactivarComponentes(patrulla3Instancia);
            AsignarSlider(patrulla3Instancia);

            yield return StartCoroutine(
                MoverAposicion(
                    patrulla3Instancia,
                    posicionPatrulla3.position,
                    velocidadEntrada
                )
            );

            ActivarPatrullaSecundaria(patrulla3Instancia);
        }

        if (patrulla2Instancia != null)
            Destroy(patrulla2Instancia);
    }
    private IEnumerator TransicionFase4()
    {
        // Patrulla 3 ya salió sola (PatrullaSecundaria.SalirYDestruir)
        // Patrulla 1 se oculta — ya no participa como objeto en Fase 4
        if (bossStateMachine != null) bossStateMachine.PausarAtaques(true);
        OcultarPatrulla1();

        yield return new WaitForSeconds(0.8f);

        GameObject izquierda = null;
        GameObject centro = null;
        GameObject derecha = null;

        if (jefeFinalIzquierdaPrefab != null)
        {
            izquierda = Instantiate(
                jefeFinalIzquierdaPrefab,
                posicionPatrulla1.position,
                Quaternion.identity
            );
        }

        yield return new WaitForSeconds(retardoFila);

        if (jefeFinalCentroPrefab != null)
        {
            centro = Instantiate(
                jefeFinalCentroPrefab,
                posicionPatrulla2.position,
                Quaternion.identity
            );
            // Asignar slider al JefeFinal y PatrullaSecundaria del centro
            JefeFinal jf = centro.GetComponent<JefeFinal>();
            if (jf != null) jf.SetSlider(hpSlider);
            AsignarSlider(centro);
        }

        yield return new WaitForSeconds(retardoFila);

        if (jefeFinalDerechaPrefab != null)
        {
            derecha = Instantiate(
                jefeFinalDerechaPrefab,
                posicionPatrulla3.position,
                Quaternion.identity
            );
        }

        GameObject controlador =
    new GameObject("PatronFaseFinal");

        PatronFaseFinal patron =
            controlador.AddComponent<PatronFaseFinal>();

        patron.izquierda =
            izquierda.GetComponent<PatrullaSecundaria>();

        patron.centro =
            centro.GetComponent<PatrullaSecundaria>();

        patron.derecha =
            derecha.GetComponent<PatrullaSecundaria>();
    }

    // ──────────────────────────────────────────────
    //  HELPERS
    // ──────────────────────────────────────────────

    private void AsignarSlider(GameObject obj)
    {
        if (obj == null || hpSlider == null) return;
        PatrullaSecundaria ps = obj.GetComponent<PatrullaSecundaria>();
        if (ps != null) ps.SetSlider(hpSlider);
    }

    private void DesactivarComponentes(GameObject obj)
    {
        if (obj == null) return;
        BossVerticalMovement mov = obj.GetComponent<BossVerticalMovement>();
        if (mov != null) mov.enabled = false;
        PatrullaSecundaria sec = obj.GetComponent<PatrullaSecundaria>();
        if (sec != null) sec.PausarAtaques(true);
    }

    private void ActivarPatrullaSecundaria(GameObject obj)
    {
        if (obj == null) return;
        PatrullaSecundaria sec = obj.GetComponent<PatrullaSecundaria>();
        if (sec != null) sec.Activar();
    }

    private IEnumerator MoverYActivar(GameObject obj, Vector3 destino)
    {
        yield return StartCoroutine(MoverAposicion(obj, destino, velocidadEntrada));
        ActivarPatrullaSecundaria(obj);
    }

    private IEnumerator MoverAposicion(GameObject obj, Vector3 destino, float velocidad)
    {
        while (obj != null && Vector3.Distance(obj.transform.position, destino) > 0.05f)
        {
            obj.transform.position = Vector3.MoveTowards(
                obj.transform.position,
                destino,
                velocidad * Time.deltaTime
            );
            yield return null;
        }
        if (obj != null)
            obj.transform.position = destino;
    }

    // Solo se usa en Fase 4 para la Patrulla 1
    private IEnumerator FadeOut(SpriteRenderer sr, float duration)
    {
        if (sr == null) yield break;
        float elapsed = 0f;
        Color original = sr.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            sr.color = new Color(original.r, original.g, original.b, Mathf.Lerp(1f, 0f, elapsed / duration));
            yield return null;
        }
        sr.color = new Color(original.r, original.g, original.b, 1f);
    }
}