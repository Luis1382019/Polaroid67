using System.Collections;
using UnityEngine;

/// <summary>
/// PatronFaseFinal — Polaroid 67
///
/// 4 patrones para el Jefe Final:
///   Patrón 1 "Pinza"     — Laterales crean un corredor, centro remata
///   Patrón 2 "Ola"       — Disparan en secuencia acelerando
///   Patrón 3 "Cruz"      — Centro abanico + laterales ráfaga simultáneos
///   Patrón 4 "Caos"      — Los 3 apuntan al jugador con desfase
/// </summary>
public class PatronFaseFinal : MonoBehaviour
{
    [Header("Referencias")]
    public PatrullaSecundaria izquierda;
    public PatrullaSecundaria centro;
    public PatrullaSecundaria derecha;

    [Header("Tiempo entre patrones")]
    public float tiempoEntrePatrones = 2f;

    private void Start()
    {
        StartCoroutine(Patrones());
    }

    private IEnumerator Patrones()
    {
        while (true)
        {
            yield return StartCoroutine(Patron_Pinza());
            yield return new WaitForSeconds(tiempoEntrePatrones);

            yield return StartCoroutine(Patron_Ola());
            yield return new WaitForSeconds(tiempoEntrePatrones);

            yield return StartCoroutine(Patron_Cruz());
            yield return new WaitForSeconds(tiempoEntrePatrones);

            yield return StartCoroutine(Patron_Caos());
            yield return new WaitForSeconds(tiempoEntrePatrones);

            yield return StartCoroutine(Patron_ForzarCarril());
            yield return new WaitForSeconds(tiempoEntrePatrones);
        }
    }

    // ──────────────────────────────────────────────
    //  PATRÓN 1 — PINZA
    //  Laterales disparan ráfagas rápidas 3 veces creando un corredor,
    //  luego el centro remata con abanico amplio + abanico cerrado.
    //  Obliga al jugador a estar en una posición específica y luego lo presiona.
    // ──────────────────────────────────────────────
    private IEnumerator Patron_Pinza()
    {
        for (int i = 0; i < 3; i++)
        {
            if (izquierda != null) izquierda.DisparoRafaga(6, 9f);
            if (derecha != null) derecha.DisparoRafaga(6, 9f);
            yield return new WaitForSeconds(0.8f);
        }

        yield return new WaitForSeconds(0.3f);

        // Centro remata con abanico amplio
        if (centro != null) centro.DisparoAbanico(9, 90f, 7f);
        yield return new WaitForSeconds(0.6f);

        // Luego abanico cerrado más rápido
        if (centro != null) centro.DisparoAbanico(4, 40f, 9f);
    }

    // ──────────────────────────────────────────────
    //  PATRÓN 2 — OLA
    //  Izquierda → Centro → Derecha en secuencia, acelerando cada ronda.
    //  4 rondas, cada una más rápida. Predecible pero exigente.
    // ──────────────────────────────────────────────
    private IEnumerator Patron_Ola()
    {
        float delay = 0.5f;

        for (int ronda = 0; ronda < 4; ronda++)
        {
            if (izquierda != null) izquierda.DisparoAbanico(4, 50f, 7f);
            yield return new WaitForSeconds(delay);

            if (centro != null) centro.DisparoAbanico(5, 65f, 8f);
            yield return new WaitForSeconds(delay);

            if (derecha != null) derecha.DisparoAbanico(4, 50f, 7f);
            yield return new WaitForSeconds(delay * 1.5f);

            // Cada ronda acelera
            delay = Mathf.Max(delay - 0.08f, 0.2f);
        }
    }

    // ──────────────────────────────────────────────
    //  PATRÓN 3 — CRUZ
    //  Centro dispara abanico de 180 grados, inmediatamente
    //  los laterales disparan ráfagas. Sin tiempo de reacción.
    //  Se repite 3 veces con pausa corta.
    // ──────────────────────────────────────────────
    private IEnumerator Patron_Cruz()
    {
        for (int i = 0; i < 3; i++)
        {
            // Centro cubre medio mapa
            if (centro != null) centro.DisparoAbanico(10, 90f, 7f);

            // Laterales rematan sin pausa
            if (izquierda != null) izquierda.DisparoRafaga(5, 8f);
            if (derecha != null) derecha.DisparoRafaga(5, 8f);

            yield return new WaitForSeconds(1.2f);
        }
    }

    // ──────────────────────────────────────────────
    //  PATRÓN 5 — FORZAR CARRIL
    //  Cada patrulla cubre su carril con una cortina de balas,
    //  dejando libre solo el carril de otra patrulla.
    //  Secuencia: carril izquierda seguro → carril centro seguro → carril derecha seguro
    // ──────────────────────────────────────────────
    private IEnumerator Patron_ForzarCarril()
    {
        // Abanico fijo hacia la izquierda — no sigue al jugador
        // El ángulo cubre el carril de la patrulla que dispara
        // dejando libre el carril de la patrulla que no dispara

        // --- FASE A: Carril izquierda (Y=3) es seguro ---
        if (centro != null) centro.DisparoAbanicoFijo(5, 60f, 7f, Vector2.left);
        if (derecha != null) derecha.DisparoAbanicoFijo(5, 60f, 7f, Vector2.left);
        yield return new WaitForSeconds(1.0f);

        if (centro != null) centro.DisparoRafaga(4, 8f);
        if (derecha != null) derecha.DisparoRafaga(4, 8f);
        yield return new WaitForSeconds(1.4f);

        // --- FASE B: Carril centro (Y=0.5) es seguro ---
        if (izquierda != null) izquierda.DisparoAbanicoFijo(5, 60f, 7f, Vector2.left);
        if (derecha != null) derecha.DisparoAbanicoFijo(5, 60f, 7f, Vector2.left);
        yield return new WaitForSeconds(1.0f);

        if (izquierda != null) izquierda.DisparoRafaga(4, 8f);
        if (derecha != null) derecha.DisparoRafaga(4, 8f);
        yield return new WaitForSeconds(1.4f);

        // --- FASE C: Carril derecha (Y=-2) es seguro ---
        if (izquierda != null) izquierda.DisparoAbanicoFijo(5, 60f, 7f, Vector2.left);
        if (centro != null) centro.DisparoAbanicoFijo(5, 60f, 7f, Vector2.left);
        yield return new WaitForSeconds(1.0f);

        if (izquierda != null) izquierda.DisparoRafaga(4, 8f);
        if (centro != null) centro.DisparoRafaga(4, 8f);
        yield return new WaitForSeconds(1.4f);

        // --- REMATE: Los 3 apuntan al jugador ---
        if (izquierda != null) izquierda.DisparoAbanico(3, 30f, 9f);
        if (centro != null) centro.DisparoAbanico(3, 30f, 9f);
        if (derecha != null) derecha.DisparoAbanico(3, 30f, 9f);
    }

    // ──────────────────────────────────────────────
    //  PATRÓN 4 — CAOS CONTROLADO
    //  Los 3 apuntan al jugador con 0.4s de desfase entre cada uno,
    //  3 veces seguidas. Difícil pero con ritmo aprendible.
    // ──────────────────────────────────────────────
    private IEnumerator Patron_Caos()
    {
        for (int i = 0; i < 3; i++)
        {
            if (izquierda != null) izquierda.DisparoAbanico(3, 30f, 8f);
            yield return new WaitForSeconds(0.4f);

            if (centro != null) centro.DisparoAbanico(5, 50f, 9f);
            yield return new WaitForSeconds(0.4f);

            if (derecha != null) derecha.DisparoAbanico(3, 30f, 8f);
            yield return new WaitForSeconds(0.8f);
        }

        // Remate final — los 3 a la vez
        yield return new WaitForSeconds(0.3f);
        if (izquierda != null) izquierda.DisparoRafaga(8, 9f);
        if (centro != null) centro.DisparoAbanico(7, 80f, 8f);
        if (derecha != null) derecha.DisparoRafaga(8, 9f);
    }
}