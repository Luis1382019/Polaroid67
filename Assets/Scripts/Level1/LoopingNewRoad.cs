using UnityEngine;

public class LoopingNewRoad : MonoBehaviour
{
    [SerializeField] private Transform roadA;
    [SerializeField] private Transform roadB;

    [Header("Movement")]
    [SerializeField] private float speed = 0f;
    [SerializeField] private float startSpeed = 0f;
    [SerializeField] private float targetSpeed = 6f;
    [SerializeField] private float accelerationTime = 3f;
    [SerializeField] private Vector2 moveDirection = Vector2.left;

    [Header("Loop")]
    [SerializeField] private float loopSize = 10f;

    private bool isRunning = true;
    private float accelerationTimer = 0f;

    private void Start()
    {
        speed = startSpeed;
    }

    private void Update()
    {
        if (!isRunning) return;

        AccelerateRoad();

        MoveRoad(roadA);
        MoveRoad(roadB);
    }

    private void AccelerateRoad()
    {
        if (speed >= targetSpeed) return;

        accelerationTimer += Time.deltaTime;

        float t = accelerationTimer / accelerationTime;
        speed = Mathf.Lerp(startSpeed, targetSpeed, t);

        if (speed > targetSpeed)
        {
            speed = targetSpeed;
        }
    }

    private void MoveRoad(Transform road)
    {
        Vector3 direction = new Vector3(moveDirection.x, moveDirection.y, 0f).normalized;

        road.position += direction * speed * Time.deltaTime;

        if (moveDirection.x < 0 && road.position.x <= -loopSize)
        {
            road.position += Vector3.right * loopSize * 2f;
        }
        else if (moveDirection.x > 0 && road.position.x >= loopSize)
        {
            road.position += Vector3.left * loopSize * 2f;
        }
        else if (moveDirection.y < 0 && road.position.y <= -loopSize)
        {
            road.position += Vector3.up * loopSize * 2f;
        }
        else if (moveDirection.y > 0 && road.position.y >= loopSize)
        {
            road.position += Vector3.down * loopSize * 2f;
        }
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public float GetSpeed()
    {
        return speed;
    }

    public void StopLoop()
    {
        isRunning = false;
    }

    public void StartLoop()
    {
        isRunning = true;
    }
}