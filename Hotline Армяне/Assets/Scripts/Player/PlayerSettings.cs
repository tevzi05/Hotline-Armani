using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerSettings : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider volumeSlider;
    public Slider brightnessSlider;

    [Header("Post Processing")]
    public Volume globalVolume;
    private ColorAdjustments _colorAdjustments;

    [Header("Audio")]
    public AudioSource bgmSource;

    //void Awake()
    //{

    //    if (globalVolume != null) globalVolume.profile.TryGet(out _colorAdjustments);
    //    float savedBrightness = PlayerPrefs.GetFloat("Brightness", 1.0f);
    //    ApplyBrightnessToScreen(savedBrightness);
    //    if (brightnessSlider != null) brightnessSlider.value = savedBrightness;

    //    // 5. То же самое для громкости
    //    float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
    //    ApplyVolumeToMusic(savedVolume);
    //    if (volumeSlider != null) volumeSlider.value = savedVolume;
    //}

    // Start теперь можно оставить пустым или удалить
    void Start() // Вместо Awake
    {
        if (globalVolume != null) globalVolume.profile.TryGet(out _colorAdjustments);

        float savedBrightness = PlayerPrefs.GetFloat("Brightness", 1.0f);
        ApplyBrightnessToScreen(savedBrightness);
        if (brightnessSlider != null) brightnessSlider.value = savedBrightness;

        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        ApplyVolumeToMusic(savedVolume);
        if (volumeSlider != null) volumeSlider.value = savedVolume;
    }


    public void SaveVolume()
    {
        float val = volumeSlider.value;
        PlayerPrefs.SetFloat("MusicVolume", volumeSlider.value);
        PlayerPrefs.Save();
        ApplyVolumeToMusic(val);
        Debug.Log("Громкость сохранена: " + volumeSlider.value);
    }

    // Этот метод привяжи к On Value Changed слайдера яркости
    public void SaveBrightness()
    {
        float val = brightnessSlider.value;
        PlayerPrefs.SetFloat("Brightness", val);
        PlayerPrefs.Save();

        // Сразу меняем яркость на экране
        ApplyBrightnessToScreen(val);

        Debug.Log("Яркость сохранена: " + val);
    }
    private void ApplyVolumeToMusic(float value)
    {
        // Вызываем ResetEffects или SetMuffled, чтобы музыка сразу применила новую громкость
        // Если игра на паузе, то передаем true в SetMuffled, если нет - ResetEffects
        if (Time.timeScale < 0.1f)
            MusicController.Instance.SetMuffled(true);
        else
            MusicController.Instance.ResetEffects();
    
    
        // 2. На всякий случай обновляем локальный источник
        if (bgmSource != null)
            bgmSource.volume = value;
            //if (bgmSource != null)
            //bgmSource.volume = value;
    }
    private void ApplyBrightnessToScreen(float value)
    {
        if (_colorAdjustments != null)
        _colorAdjustments.postExposure.value = Mathf.Lerp(-3f, 3f, value);
    }
}
