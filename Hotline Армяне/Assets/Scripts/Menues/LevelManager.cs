using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    // Метод для кнопок
    public void PlayGameSolo()
    {
        SceneManager.LoadScene("SampleScene");
        if (MusicController.Instance != null) MusicController.Instance.StartMusic();

    }

    public void PlayGameExtraction()
    {
        SceneManager.LoadScene("Extraction");
    }
}