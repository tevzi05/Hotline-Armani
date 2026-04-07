using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class PauseMenu : MonoBehaviour
{
    public bool PauseGame;

    private GameObject pauseGameMenu;
    private GameObject crosshair;     

    void Start()
    {
        //Ищем меню по имени объекта в иерархии
        Transform menuTransform = transform.Find("PauseMenu");
        if (menuTransform != null) pauseGameMenu = menuTransform.gameObject;
        crosshair = GameObject.FindGameObjectWithTag("Crosshair");

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (PauseGame) Resume();
            else Pause();
        }
    }

    public void Resume()
    {
        if (pauseGameMenu != null) pauseGameMenu.SetActive(false);
        if (crosshair != null) crosshair.SetActive(true); // Включаем прицел 

        Time.timeScale = 1f;
        PauseGame = false;
    }

    public void Pause()
    {
        if (pauseGameMenu != null) pauseGameMenu.SetActive(true);
        if (crosshair != null) crosshair.SetActive(false); // Выключаем прицел

        Time.timeScale = 0;
        PauseGame = true;
        Cursor.visible = true;
    }

    public void LosdMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}
