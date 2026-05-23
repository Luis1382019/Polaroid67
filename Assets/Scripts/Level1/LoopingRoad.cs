using UnityEngine;

public class LoopingRoad : MonoBehaviour
{
    [SerializeField] private Transform roadA;
    [SerializeField] private Transform roadB;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float roadHeight = 10f;

    private bool isRunning = true;

    private void Update()
    {
        if (!isRunning) return;

        MoveRoad(roadA);
        MoveRoad(roadB);
    }

    private void MoveRoad(Transform road)
    {
        road.position += Vector3.down * speed * Time.deltaTime;

        if (road.position.y <= -roadHeight)
        {
            road.position += Vector3.up * roadHeight * 2f;
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