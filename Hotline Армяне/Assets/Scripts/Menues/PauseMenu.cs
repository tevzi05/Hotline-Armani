using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class PauseMenu : MonoBehaviour
{
    public bool PauseGame;
    public AudioSource bgm;
    private GameObject pauseGameMenu;
    private GameObject crosshair;
    private float originalVolume;
    public float targetPitch = 1f; // Целевая высота звука
    public float pitchChangeSpeed = 5f; // Скорость изменения

    void Start()
    {
        //Ищем меню по имени объекта в иерархии
        Transform menuTransform = transform.Find("PauseMenu");
        if (menuTransform != null) pauseGameMenu = menuTransform.gameObject;
        crosshair = GameObject.FindGameObjectWithTag("Crosshair");
        if (bgm != null) originalVolume = bgm.volume;

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (PauseGame) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        if (bgm != null)
        {
            bgm.pitch = 0.6f;
            bgm.volume = originalVolume * 0.3f;
        }
        Time.timeScale = 0;
        if (pauseGameMenu != null) pauseGameMenu.SetActive(true);
        if (crosshair != null) crosshair.SetActive(false); // Выключаем прицел
        PauseGame = true;
        Cursor.visible = true;
    }

    public void Resume()
    {
        if (pauseGameMenu != null) pauseGameMenu.SetActive(false);
        if (crosshair != null) crosshair.SetActive(true); // Включаем прицел
        if (bgm != null)
        {
            bgm.pitch = 1f;
            bgm.volume = originalVolume;
        }
        Time.timeScale = 1f;
        PauseGame = false;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene");
    }

    public void LosdMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}
