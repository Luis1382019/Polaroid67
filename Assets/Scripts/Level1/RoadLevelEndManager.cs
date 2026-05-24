using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoadLevelEndManager : MonoBehaviour
{
    [Header("Level Timing")]
    [SerializeField] private float levelDuration = 30f;
    [SerializeField] private float slowDownDuration = 4f;

    [Header("Systems")]
    [SerializeField] private LoopingNewRoad roadLoop;
    [SerializeField] private RandomCarSpawner carSpawner;

    [Header("End UI")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private float delayBeforeLeaving = 2f;

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName = "ChapterSelect";

    private bool levelEnding = false;

    private void Start()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }

        StartCoroutine(LevelTimer());
    }

    private IEnumerator LevelTimer()
    {
        yield return new WaitForSeconds(levelDuration);

        if (!levelEnding)
        {
            StartCoroutine(EndLevelSequence());
        }
    }

    private IEnumerator EndLevelSequence()
    {
        levelEnding = true;

        if (carSpawner != null)
        {
            carSpawner.enabled = false;
        }

        DamageCarObstacle[] cars = Object.FindObjectsByType<DamageCarObstacle>(
            FindObjectsInactive.Exclude
        );

        float roadStartSpeed = roadLoop.GetSpeed();

        float[] carStartSpeeds = new float[cars.Length];

        for (int i = 0; i < cars.Length; i++)
        {
            carStartSpeeds[i] = cars[i].GetSpeed();
        }

        float timer = 0f;

        while (timer < slowDownDuration)
        {
            timer += Time.deltaTime;

            float t = timer / slowDownDuration;

            float newRoadSpeed = Mathf.Lerp(roadStartSpeed, 0f, t);
            roadLoop.SetSpeed(newRoadSpeed);

            for (int i = 0; i < cars.Length; i++)
            {
                if (cars[i] != null)
                {
                    float newCarSpeed = Mathf.Lerp(carStartSpeeds[i], 0f, t);
                    cars[i].SetSpeed(newCarSpeed);
                }
            }

            yield return null;
        }

        roadLoop.SetSpeed(0f);
        roadLoop.StopLoop();

        foreach (DamageCarObstacle car in cars)
        {
            if (car != null)
            {
                car.SetSpeed(0f);
                car.StopMovement();
            }
        }

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }

        yield return new WaitForSeconds(delayBeforeLeaving);

        SceneManager.LoadScene(nextSceneName);
    }
}