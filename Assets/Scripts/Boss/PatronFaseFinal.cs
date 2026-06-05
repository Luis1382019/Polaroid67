using System.Collections;
using UnityEngine;

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
            yield return StartCoroutine(Patron1());
            yield return new WaitForSeconds(tiempoEntrePatrones);

            yield return StartCoroutine(Patron2());
            yield return new WaitForSeconds(tiempoEntrePatrones);

            yield return StartCoroutine(Patron3());
            yield return new WaitForSeconds(tiempoEntrePatrones);

            yield return StartCoroutine(Patron4());
            yield return new WaitForSeconds(tiempoEntrePatrones);
        }
    }

    private IEnumerator Patron1()
    {
        for (int i = 0; i < 3; i++)
        {
            izquierda.DisparoRafaga(10, 8f);
            derecha.DisparoRafaga(10, 8f);

            yield return new WaitForSeconds(1f);
        }

        centro.DisparoAbanico(9, 120f, 6f);

        yield return new WaitForSeconds(1f);

        centro.DisparoAbanico(3, 45f, 6f);
    }

    private IEnumerator Patron2()
    {
        for (int i = 0; i < 3; i++)
        {
            centro.DisparoRafaga(10, 8f);

            yield return new WaitForSeconds(1f);
        }

        izquierda.DisparoAbanico(5, 90f, 6f);
        derecha.DisparoAbanico(5, 90f, 6f);
    }

    private IEnumerator Patron3()
    {
        for (int i = 0; i < 3; i++)
        {
            centro.DisparoRafaga(10, 8f);
            derecha.DisparoRafaga(10, 8f);

            yield return new WaitForSeconds(1f);
        }

        izquierda.DisparoAbanico(3, 60f, 6f);
    }

    private IEnumerator Patron4()
    {
        for (int i = 0; i < 3; i++)
        {
            centro.DisparoRafaga(10, 8f);
            izquierda.DisparoRafaga(10, 8f);

            yield return new WaitForSeconds(1f);
        }

        derecha.DisparoAbanico(3, 60f, 6f);
    }
}