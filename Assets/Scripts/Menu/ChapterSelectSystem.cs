using UnityEngine;
using UnityEngine.SceneManagement;

public class ChapterSelectSystem : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "Menu";

    public void GoBackToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}