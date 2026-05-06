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

    void Awake()
    {
        
        if (globalVolume != null) globalVolume.profile.TryGet(out _colorAdjustments);
        float savedBrightness = PlayerPrefs.GetFloat("Brightness", 1.0f);
        ApplyBrightnessToScreen(savedBrightness);
        if (brightnessSlider != null) brightnessSlider.value = savedBrightness;

        // 5. “о же самое дл€ громкости
        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        ApplyVolumeToMusic(savedVolume);
        if (volumeSlider != null) volumeSlider.value = savedVolume;
    }

    // Start теперь можно оставить пустым или удалить


    public void SaveVolume()
    {
        float val = volumeSlider.value;
        PlayerPrefs.SetFloat("MusicVolume", volumeSlider.value);
        PlayerPrefs.Save();
        ApplyVolumeToMusic(val);
        Debug.Log("√ромкость сохранена: " + volumeSlider.value);
    }

    // Ётот метод прив€жи к On Value Changed слайдера €ркости
    public void SaveBrightness()
    {
        float val = brightnessSlider.value;
        PlayerPrefs.SetFloat("Brightness", val);
        PlayerPrefs.Save();

        // —разу мен€ем €ркость на экране
        ApplyBrightnessToScreen(val);

        Debug.Log("яркость сохранена: " + val);
    }
    private void ApplyVolumeToMusic(float value)
    {
        if (bgmSource != null)
        bgmSource.volume = value;
    }
    private void ApplyBrightnessToScreen(float value)
    {
        if (_colorAdjustments != null)
        _colorAdjustments.postExposure.value = Mathf.Lerp(-3f, 3f, value);
    }
}
