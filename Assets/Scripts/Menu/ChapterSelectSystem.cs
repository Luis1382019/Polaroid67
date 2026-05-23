using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ChapterSelectSystem : MonoBehaviour
{
    [System.Serializable]
    public class ChapterData
    {
        public string chapterTitle;
        public string sceneName;
        public Sprite backgroundSprite;
    }

    [Header("Chapters")]
    [SerializeField] private ChapterData[] chapters;
    [SerializeField] private int selectedChapterIndex = 0;

    [Header("UI")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text chapterTitleText;

    [Header("Scenes")]
    [SerializeField] private string menuSceneName = "menu";

    private void Start()
    {
        SelectChapter(selectedChapterIndex);
    }

    public void SelectChapter(int chapterIndex)
    {
        if (chapterIndex < 0 || chapterIndex >= chapters.Length)
        {
            Debug.LogWarning("Índice de capítulo inválido: " + chapterIndex);
            return;
        }

        selectedChapterIndex = chapterIndex;

        ChapterData selectedChapter = chapters[selectedChapterIndex];

        backgroundImage.sprite = selectedChapter.backgroundSprite;
        chapterTitleText.text = selectedChapter.chapterTitle;
    }

    public void PlaySelectedChapter()
    {
        SceneManager.LoadScene(chapters[selectedChapterIndex].sceneName);
    }

    public void GoBackToMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }
}