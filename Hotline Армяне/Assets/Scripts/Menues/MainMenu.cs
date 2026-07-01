using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject levelSelectPanelSolo;
    public GameObject levelSelectPanelExtraction;
    public void ChooseLevelSolo()
    {
        mainMenu.SetActive(false);
        levelSelectPanelSolo.SetActive(true);

    }

    public void ChooseLevelExtraction()
    {
        mainMenu.SetActive(false);
        levelSelectPanelExtraction.SetActive(true);
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
