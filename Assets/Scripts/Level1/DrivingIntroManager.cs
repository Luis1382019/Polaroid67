using System.Collections;
using UnityEngine;

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
    [SerializeField] private GameObject brokenCarDialogueManager;

    private bool hasEnded = false;

    private void Start()
    {
        carNormal.SetActive(true);
        carBroken.SetActive(false);

        if (brokenCarDialogueManager != null)
        {
            brokenCarDialogueManager.SetActive(false);
        }
    }

    private void Update()
    {
        if (hasEnded) return;

        if (dialogueSystem.DialogueFinished)
        {
            hasEnded = true;
            StartCoroutine(SlowDownAndBreakCar());
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

        if (brokenCarDialogueManager != null)
        {
            brokenCarDialogueManager.SetActive(true);
        }

        Debug.Log("Empieza el diálogo después de la avería.");
    }
}