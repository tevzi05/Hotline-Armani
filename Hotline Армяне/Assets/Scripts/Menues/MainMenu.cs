using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
   public void PlayGame()
   {
        SceneManager.LoadScene("SampleScene");
        if (MusicController.Instance != null) MusicController.Instance.StartMusic();

    }
    public void ExitGame()
    {
        Application.Quit();
    }
}
