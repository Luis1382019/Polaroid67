using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PatrullaSpawnSystem : MonoBehaviour
{
    [Header("Prefabs — Fases 2 y 3")]
    [SerializeField] private GameObject patrulla2Prefab;
    [SerializeField] private GameObject patrulla3Prefab;

    [Header("Prefabs — Fase 4 (Jefe Final)")]
    [SerializeField] private GameObject jefeFinalCentroPrefab;
    [SerializeField] private GameObject jefeFinalIzquierdaPrefab;
    [SerializeField] private GameObject jefeFinalDerechaPrefab;

    [Header("Puntos de Spawn (solo entrada)")]
    [SerializeField] private Transform spawnPointDerecha;
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
    private CanvasGroup hpSliderCanvasGroup;

    private void Awake()
    {
        bossStateMachine = GetComponent<BossStateMachine>();
        bossMovimiento = GetComponent<BossVerticalMovement>();
        bossRenderer = GetComponent<SpriteRenderer>();
        bossCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (hpSlider != null)
        {
            hpSliderCanvasGroup = hpSlider.GetComponent<CanvasGroup>();
            if (hpSliderCanvasGroup == null)
                hpSliderCanvasGroup = hpSlider.gameObject.AddComponent<CanvasGroup>();
        }
    }

    // ──────────────────────────────────────────────
    //  API PÚBLICA
    // ──────────────────────────────────────────────

    public void IniciarFase1() { StartCoroutine(EntradaFase1()); }
    public void IniciarFase2() { StartCoroutine(TransicionFase2()); }
    public void IniciarFase3() { StartCoroutine(TransicionFase3()); }
    public void IniciarFase4() { StartCoroutine(TransicionFase4()); }

    public void LimpiarPatrullas()
    {
        if (patrulla2Instancia != null) Destroy(patrulla2Instancia);
        if (patrulla3Instancia != null) Destroy(patrulla3Instancia);
    }

    // ──────────────────────────────────────────────
    //  SLIDER
    // ──────────────────────────────────────────────

    private void OcultarSlider()
    {
        if (hpSliderCanvasGroup != null) hpSliderCanvasGroup.alpha = 0f;
    }

    private void MostrarSlider()
    {
        if (hpSliderCanvasGroup != null) hpSliderCanvasGroup.alpha = 1f;
    }

    private void AsignarSlider(GameObject obj)
    {
        if (obj == null || hpSlider == null) return;
        PatrullaSecundaria ps = obj.GetComponent<PatrullaSecundaria>();
        if (ps != null) ps.SetSlider(hpSlider);
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

    private IEnumerator EntradaFase1()
    {
        // Patrulla 1 empieza fuera de pantalla a la derecha
        if (bossCollider != null) bossCollider.enabled = false;
        if (bossMovimiento != null) bossMovimiento.enabled = false;
        if (bossStateMachine != null) bossStateMachine.PausarAtaques(true);

        patrulla1.transform.position = new Vector3(
            spawnPointDerecha.position.x,
            posicionPatrulla1.position.y,
            posicionPatrulla1.position.z
        );

        // Entra hasta su posición de combate
        yield return StartCoroutine(MoverAposicion(patrulla1, posicionPatrulla1.position, velocidadEntrada));

        // Ya en posición, activar todo
        if (bossCollider != null) bossCollider.enabled = true;
        if (bossMovimiento != null) bossMovimiento.enabled = true;
        if (bossStateMachine != null) bossStateMachine.PausarAtaques(false);
    }

    private IEnumerator TransicionFase2()
    {
        if (bossStateMachine != null) bossStateMachine.PausarAtaques(true);
        if (bossMovimiento != null) bossMovimiento.enabled = false;
        if (bossCollider != null) bossCollider.enabled = false;

        Vector3 salidaP1 = new Vector3(
            spawnPointDerecha.position.x + 3f,
            patrulla1.transform.position.y,
            patrulla1.transform.position.z
        );
        yield return StartCoroutine(MoverAposicion(patrulla1, salidaP1, velocidadSalida));
        OcultarPatrulla1();
        OcultarSlider();

        yield return new WaitForSeconds(0.1f);

        if (patrulla2Prefab != null && spawnPointDerecha != null)
        {
            Vector3 spawnPos = new Vector3(
                spawnPointDerecha.position.x,
                posicionPatrulla2.position.y,
                spawnPointDerecha.position.z
            );
            patrulla2Instancia = Instantiate(patrulla2Prefab, spawnPos, Quaternion.identity);
            DesactivarComponentes(patrulla2Instancia);
            AsignarSlider(patrulla2Instancia);
            yield return StartCoroutine(MoverAposicion(patrulla2Instancia, posicionPatrulla2.position, velocidadEntrada));
            ActivarPatrullaSecundaria(patrulla2Instancia);
            MostrarSlider();
        }
    }

    private IEnumerator TransicionFase3()
    {
        if (patrulla2Instancia != null)
        {
            DesactivarComponentes(patrulla2Instancia);
            Vector3 salidaP2 = new Vector3(
                spawnPointDerecha.position.x,
                patrulla2Instancia.transform.position.y,
                patrulla2Instancia.transform.position.z
            );
            StartCoroutine(MoverAposicion(patrulla2Instancia, salidaP2, velocidadSalida));
        }

        yield return new WaitForSeconds(0.25f);
        OcultarSlider();

        if (patrulla3Prefab != null && spawnPointDerecha != null)
        {
            Vector3 spawnPos = new Vector3(
                spawnPointDerecha.position.x,
                posicionPatrulla3.position.y,
                spawnPointDerecha.position.z
            );
            patrulla3Instancia = Instantiate(patrulla3Prefab, spawnPos, Quaternion.identity);
            DesactivarComponentes(patrulla3Instancia);
            AsignarSlider(patrulla3Instancia);
            yield return StartCoroutine(MoverAposicion(patrulla3Instancia, posicionPatrulla3.position, velocidadEntrada));
            ActivarPatrullaSecundaria(patrulla3Instancia);
            MostrarSlider();
        }

        if (patrulla2Instancia != null)
            Destroy(patrulla2Instancia);
    }

    private IEnumerator TransicionFase4()
    {
        if (bossStateMachine != null) bossStateMachine.PausarAtaques(true);
        OcultarPatrulla1();
        OcultarSlider();

        yield return new WaitForSeconds(0.8f);

        GameObject izquierda = null;
        GameObject centro = null;
        GameObject derecha = null;

        // Izquierda entra primero
        if (jefeFinalIzquierdaPrefab != null)
        {
            Vector3 spawnIzq = new Vector3(spawnPointDerecha.position.x, posicionPatrulla1.position.y, posicionPatrulla1.position.z);
            izquierda = Instantiate(jefeFinalIzquierdaPrefab, spawnIzq, Quaternion.identity);
            yield return StartCoroutine(MoverAposicion(izquierda, posicionPatrulla1.position, velocidadEntrada));
        }

        yield return new WaitForSeconds(retardoFila);

        // Derecha entra segunda
        if (jefeFinalDerechaPrefab != null)
        {
            Vector3 spawnDer = new Vector3(spawnPointDerecha.position.x, posicionPatrulla3.position.y, posicionPatrulla3.position.z);
            derecha = Instantiate(jefeFinalDerechaPrefab, spawnDer, Quaternion.identity);
            yield return StartCoroutine(MoverAposicion(derecha, posicionPatrulla3.position, velocidadEntrada));
        }

        yield return new WaitForSeconds(retardoFila);

        // Centro entra último y arranca ataques
        if (jefeFinalCentroPrefab != null)
        {
            Vector3 spawnCen = new Vector3(spawnPointDerecha.position.x, posicionPatrulla2.position.y, posicionPatrulla2.position.z);
            centro = Instantiate(jefeFinalCentroPrefab, spawnCen, Quaternion.identity);
            // Esperar 2 frames para que Start() de JefeFinal inicialice sharedHealth
            yield return null;
            yield return null;
            JefeFinal jf = centro.GetComponent<JefeFinal>();
            if (jf != null) jf.SetSlider(hpSlider);
            AsignarSlider(centro);
            yield return StartCoroutine(MoverAposicion(centro, posicionPatrulla2.position, velocidadEntrada));
            // NO llamar Activar() — PatronFaseFinal controla los disparos
            MostrarSlider();
        }

        if (centro != null)
        {
            GameObject controlador = new GameObject("PatronFaseFinal");
            PatronFaseFinal patron = controlador.AddComponent<PatronFaseFinal>();
            patron.izquierda = izquierda?.GetComponent<PatrullaSecundaria>();
            patron.centro = centro.GetComponent<PatrullaSecundaria>();
            patron.derecha = derecha?.GetComponent<PatrullaSecundaria>();
        }
    }

    // ──────────────────────────────────────────────
    //  HELPERS
    // ──────────────────────────────────────────────

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