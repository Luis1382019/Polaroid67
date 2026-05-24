using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DrivingIntroManager : MonoBehaviour
{
    [SerializeField] private LoopingRoad loopingRoad;
    [SerializeField] private DrivingDialogueSystem dialogueSystem;

    [Header("Cars")]
    [SerializeField] private GameObject carNormal;
    [SerializeField] private GameObject carBroken;

    [Header("Stop Settings")]
    [SerializeField] private float slowDownDuration = 2f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip carBreakSound;
    [SerializeField] private int soundRepeats = 2;
    [SerializeField] private float timeBetweenSounds = 1f;

    [Header("After Breakdown")]
    [SerializeField] private GameObject postDrivingDialogueManager;
    [SerializeField] private DrivingDialogueSystem postDrivingDialogueSystem;

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName = "Level1_GasolineraGameplay";
    [SerializeField] private float delayBeforeNextScene = 1f;

    private bool hasEnded = false;
    private bool postDialogueStarted = false;
    private bool sceneLoadStarted = false;

    private void Start()
    {
        carNormal.SetActive(true);
        carBroken.SetActive(false);

        if (postDrivingDialogueManager != null)
        {
            postDrivingDialogueManager.SetActive(false);
        }
    }

    private void Update()
    {
        if (!hasEnded && dialogueSystem.DialogueFinished)
        {
            hasEnded = true;
            StartCoroutine(SlowDownAndBreakCar());
        }

        if (postDialogueStarted && !sceneLoadStarted && postDrivingDialogueSystem.DialogueFinished)
        {
            sceneLoadStarted = true;
            StartCoroutine(LoadNextScene());
        }
    }

    private IEnumerator SlowDownAndBreakCar()
    {
        float startSpeed = loopingRoad.GetSpeed();
        float timer = 0f;

        while (timer < slowDownDuration)
        {
            timer += Time.deltaTime;

            float t = timer / slowDownDuration;
            float newSpeed = Mathf.Lerp(startSpeed, 0f, t);

            loopingRoad.SetSpeed(newSpeed);

            yield return null;
        }

        loopingRoad.SetSpeed(0f);
        loopingRoad.StopLoop();

        carNormal.SetActive(false);
        carBroken.SetActive(true);

        if (audioSource != null && carBreakSound != null)
        {
            for (int i = 0; i < soundRepeats; i++)
            {
                audioSource.PlayOneShot(carBreakSound);
                yield return new WaitForSeconds(timeBetweenSounds);
            }
        }

        if (postDrivingDialogueManager != null)
        {
            postDrivingDialogueManager.SetActive(true);
            postDialogueStarted = true;
        }

        Debug.Log("Empieza el diálogo después de la avería.");
    }

    private IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(delayBeforeNextScene);

        SceneManager.LoadScene(nextSceneName);
    }
}