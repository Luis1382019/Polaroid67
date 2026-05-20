using UnityEngine;

public class TestCompleteChapter : MonoBehaviour
{
    [SerializeField] private string completedKey;
    [SerializeField] private PolaroidLevelButton polaroidToUpdate;

    public void CompleteChapter()
    {
        PlayerPrefs.SetInt(completedKey, 1);
        PlayerPrefs.Save();

        polaroidToUpdate.UpdateVisual();
    }

    public void ResetChapter()
    {
        PlayerPrefs.DeleteKey(completedKey);
        PlayerPrefs.Save();

        polaroidToUpdate.UpdateVisual();
    }
}