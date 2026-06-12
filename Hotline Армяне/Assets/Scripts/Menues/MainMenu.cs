using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject levelSelectPanel;
    public void ChooseLevel()
   {
        mainMenu.SetActive(false);
        levelSelectPanel.SetActive(true);

    }

    public void Extraction()
    {
        SceneManager.LoadScene("Extraction");
        if (MusicController.Instance != null) MusicController.Instance.StartMusic();
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
