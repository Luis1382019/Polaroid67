using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoadKillObjective : MonoBehaviour
{
    [Header("Objective")]
    [SerializeField] private int requiredKills = 10;

    [Header("Systems")]
    [SerializeField] private LoopingNewRoad roadLoop;
    [SerializeField] private RandomCarSpawner carSpawner;

    [Header("End")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private string nextSceneName = "ChapterSelect";
    [SerializeField] private float delayBeforeLeaving = 2f;

    private int currentKills = 0;
    private bool levelCompleted = false;

    private void Start()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
    }

    public void AddKill()
    {
        if (levelCompleted) return;

        currentKills++;

        Debug.Log("Carros destruidos: " + currentKills + "/" + requiredKills);

        if (currentKills >= requiredKills)
        {
            StartCoroutine(CompleteLevel());
        }
    }

    private IEnumerator CompleteLevel()
    {
        levelCompleted = true;

        if (carSpawner != null)
        {
            carSpawner.enabled = false;
        }

        if (roadLoop != null)
        {
            roadLoop.StopLoop();
        }

        DamageCarObstacle[] cars = Object.FindObjectsByType<DamageCarObstacle>(
            FindObjectsInactive.Exclude
        );

        foreach (DamageCarObstacle car in cars)
        {
            car.StopMovement();
        }

        PlayerMovement playerMovement = Object.FindAnyObjectByType<PlayerMovement>();

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        PlayerShooting playerShooting = Object.FindAnyObjectByType<PlayerShooting>();

        if (playerShooting != null)
        {
            playerShooting.enabled = false;
        }

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }

        yield return new WaitForSeconds(delayBeforeLeaving);

        SceneManager.LoadScene(nextSceneName);
    }
}