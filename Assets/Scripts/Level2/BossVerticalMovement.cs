using UnityEngine;

public class BossVerticalMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float minY = -2f;
    [SerializeField] private float maxY = 2f;

    private int direction = 1;

    private void Update()
    {
        transform.position += Vector3.up * direction * moveSpeed * Time.deltaTime;

        if (transform.position.y >= maxY)
        {
            direction = -1;
        }
        else if (transform.position.y <= minY)
        {
            direction = 1;
        }
    }
}