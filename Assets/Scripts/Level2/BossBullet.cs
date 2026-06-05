using UnityEngine;

/// <summary>
/// BossBullet — Polaroid 67
/// 
/// Versión actualizada: agrega SetSpeed() para que BossStateMachine
/// pueda controlar la velocidad por patrón.
/// El resto del comportamiento es idéntico al original.
/// </summary>
public class BossBullet : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private int damage = 1;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // Se mueve en la dirección de su rotación (hacia la izquierda por defecto)
        transform.position += transform.right * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// BossStateMachine llama esto justo después de instanciar la bala
    /// para asignar la velocidad del patrón activo.
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
}
