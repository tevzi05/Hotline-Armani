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
        if (MusicController.Instance != null) MusicController.Instance.SetMuffled(true);

        Time.timeScale = 0;
        if (pauseGameMenu != null) pauseGameMenu.SetActive(true);
        if (crosshair != null) crosshair.SetActive(false);
        PauseGame = true;
        Cursor.visible = true;
    }

    public void Resume()
    {
        if (MusicController.Instance != null) MusicController.Instance.SetMuffled(false);

        Time.timeScale = 1f;
        if (pauseGameMenu != null) pauseGameMenu.SetActive(false);
        if (crosshair != null) crosshair.SetActive(true);
        PauseGame = false;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        if (MusicController.Instance != null) MusicController.Instance.RestartMusic();
        SceneManager.LoadScene("SampleScene");
    }

    public void LosdMenu()
    {
        Time.timeScale = 1f;
        if (MusicController.Instance != null) MusicController.Instance.StopMusic();
        SceneManager.LoadScene("Menu");
    }
}
