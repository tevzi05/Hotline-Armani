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
    public void ExitGame()
    {
        Application.Quit();
    }
}
