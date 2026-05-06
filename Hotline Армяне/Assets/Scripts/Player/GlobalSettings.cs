using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlobalSettings : MonoBehaviour
{
    public static GlobalSettings Instance; // Позволит обращаться к скрипту из любой сцены

    [Header("Links")]
    private AudioSource bgmSource;
    private ColorAdjustments _colorAdjustments;

    void Awake()
    {
        // Делаем объект бессмертным
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        ApplyAllSettings();
    }

    // Этот метод будем вызывать при старте каждой сцены
    public void InitForNewScene(Slider bSlider, Slider vSlider, Volume sceneVolume)
    {
        // Находим музыку (она же у нас в DDOL)
        if (bgmSource == null)
            bgmSource = GameObject.FindWithTag("Music")?.GetComponent<AudioSource>();

        // Находим настройки цвета в новой сцене
        if (sceneVolume != null)
            sceneVolume.profile.TryGet(out _colorAdjustments);

        // Загружаем значения
        float bVal = PlayerPrefs.GetFloat("Brightness", 1.0f);
        float vVal = PlayerPrefs.GetFloat("MusicVolume", 0.5f);

        // Применяем
        ApplyBrightness(bVal);
        ApplyVolume(vVal);

        // Связываем ползунки, если они есть в этой сцене
        if (bSlider != null) bSlider.value = bVal;
        if (vSlider != null) vSlider.value = vVal;
    }

    public void ApplyBrightness(float val)
    {
        if (_colorAdjustments != null)
            _colorAdjustments.postExposure.value = Mathf.Lerp(-3f, 3f, val);
        PlayerPrefs.SetFloat("Brightness", val);
    }

    public void ApplyVolume(float val)
    {
        if (bgmSource != null)
            bgmSource.volume = val;
        PlayerPrefs.SetFloat("MusicVolume", val);
    }

    private void ApplyAllSettings()
    {
        float bVal = PlayerPrefs.GetFloat("Brightness", 1.0f);
        float vVal = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        ApplyBrightness(bVal);
        ApplyVolume(vVal);
    }
}
