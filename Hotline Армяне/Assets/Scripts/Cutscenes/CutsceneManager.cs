using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    [Header("Настройки UI")]
    public GameObject cutscenePanel; // Объект самой картинки (или весь второй Canvas)
    public Image displayImage;       // Компонент Image, где будем менять спрайты
    public GameObject crosshair; // Прицел, который нужно отключить
    public AudioSource bgm;

    [Header("Контент")]
    public Sprite slide1;
    public Sprite slide2;
    public float timePerSlide = 1f;

    void Start()
    {
       
        StartCoroutine(ExecuteCutscene());

    }

    IEnumerator ExecuteCutscene()
    {

        // 1. Ставим игру на паузу
        Time.timeScale = 0f;
        bgm = GetComponent<AudioSource>();
        bgm.pitch = 1f;
        cutscenePanel.SetActive(true);
        if (crosshair != null) crosshair.SetActive(false);

        // 2. Первая картинка
        displayImage.sprite = slide1;
        yield return new WaitForSecondsRealtime(timePerSlide); // Ждем в реальном времени

        // 3. Вторая картинка
        displayImage.sprite = slide2;
        yield return new WaitForSecondsRealtime(timePerSlide);

        // 4. Убираем катсцену и запускаем время
        cutscenePanel.SetActive(false);
        Time.timeScale = 1f;
        crosshair.SetActive(true);
    }
}