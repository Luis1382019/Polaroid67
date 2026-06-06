using System.Collections;
using UnityEngine;

/// <summary>
/// JefeLateral — Polaroid 67
/// Solo maneja el flash visual al recibir daño.
/// El daño real se aplica al JefeFinal maestro (centro).
/// </summary>
public class JefeLateral : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private float hitFlashTime = 0.08f;

    private Color originalColor;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void TakeDamage(int damage)
    {
        // Buscar el maestro y aplicar daño ahí
        JefeFinal maestro = Object.FindAnyObjectByType<JefeFinal>();
        if (maestro != null)
            maestro.TakeDamage(damage);

        StartCoroutine(HitFlash());
    }

    private IEnumerator HitFlash()
    {
        if (spriteRenderer == null) yield break;
        spriteRenderer.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashTime);
        spriteRenderer.color = originalColor;
    }
}