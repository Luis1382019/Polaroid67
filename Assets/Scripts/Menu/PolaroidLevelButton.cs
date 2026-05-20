using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PolaroidLevelButton : MonoBehaviour
{
    [SerializeField] private string levelSceneName;
    [SerializeField] private string completedKey;

    [SerializeField] private Image polaroidImage;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite unlockedSprite;

    private void Start()
    {
        UpdateVisual();
    }

    public void LoadLevel()
    {
        SceneManager.LoadScene(levelSceneName);
    }

    public void UpdateVisual()
    {
        bool completed = PlayerPrefs.GetInt(completedKey, 0) == 1;

        if (completed)
        {
            polaroidImage.sprite = unlockedSprite;
        }
        else
        {
            polaroidImage.sprite = lockedSprite;
        }
    }
}