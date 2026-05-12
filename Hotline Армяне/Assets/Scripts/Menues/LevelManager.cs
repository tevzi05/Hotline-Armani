using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    // Метод для кнопок
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene");
        if (MusicController.Instance != null) MusicController.Instance.StartMusic();

    }
}