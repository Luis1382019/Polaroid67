using UnityEngine;

/// <summary>
/// BossVerticalMovement — Polaroid 67 (versión con SetMoveSpeed)
///
/// Cambio respecto al original: agrega SetMoveSpeed() para que
/// BossStateMachine pueda acelerar el movimiento en fases avanzadas.
/// Todo lo demás es idéntico.
/// </summary>
public class BossVerticalMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float minY = -2f;
    [SerializeField] private float maxY = 2f;

    private int direction = 1;

    private void Update()
    {
        Debug.Log($"[BossVertMovement] Y={transform.position.y} | enabled={enabled}");

        transform.position += Vector3.up * direction * moveSpeed * Time.deltaTime;

        if (transform.position.y >= maxY)
            direction = -1;
        else if (transform.position.y <= minY)
            direction = 1;
    }

    /// <summary>
    /// BossStateMachine llama esto al entrar a cada fase nueva.
    /// </summary>
    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }
}
