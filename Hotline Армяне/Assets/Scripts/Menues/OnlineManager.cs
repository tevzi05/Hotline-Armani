using UnityEngine;
using UnityEngine.SceneManagement;

public class OnlineManager : MonoBehaviour
{
    // Метод для кнопок
    public void PlayOnlineGame()
    {
        SceneManager.LoadScene("MultiplayerScene");
        if (MusicController.Instance != null) MusicController.Instance.StartMusic();

    }
}