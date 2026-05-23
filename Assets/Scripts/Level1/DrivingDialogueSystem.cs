using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DrivingDialogueSystem : MonoBehaviour
{
    public enum Speaker
    {
        Protagonist,
        Friend
    }

    [System.Serializable]
    public class DialogueLine
    {
        public Speaker speaker;

        [TextArea(2, 4)]
        public string text;
    }

    [Header("UI")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image speakerImage;

    [Header("Speaker Sprites")]
    [SerializeField] private Sprite protagonistSprite;
    [SerializeField] private Sprite friendSprite;

    [Header("Dialogue")]
    [SerializeField] private DialogueLine[] dialogueLines;

    private int currentLineIndex = 0;
    private bool dialogueFinished = false;

    public bool DialogueFinished => dialogueFinished;

    private void OnEnable()
    {
        currentLineIndex = 0;
        dialogueFinished = false;
        ShowCurrentLine();
    }

    private void Update()
    {
        if (dialogueFinished) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            NextLine();
        }
    }

    private void ShowCurrentLine()
    {
        if (dialogueLines.Length == 0)
        {
            dialogueText.text = "";
            speakerImage.gameObject.SetActive(false);
            dialogueFinished = true;
            return;
        }

        speakerImage.gameObject.SetActive(true);

        DialogueLine currentLine = dialogueLines[currentLineIndex];

        dialogueText.text = currentLine.text;

        if (currentLine.speaker == Speaker.Protagonist)
        {
            speakerImage.sprite = protagonistSprite;
        }
        else
        {
            speakerImage.sprite = friendSprite;
        }
    }

    private void NextLine()
    {
        currentLineIndex++;

        if (currentLineIndex >= dialogueLines.Length)
        {
            dialogueFinished = true;
            dialogueText.text = "";
            speakerImage.gameObject.SetActive(false);
            return;
        }

        ShowCurrentLine();
    }
}