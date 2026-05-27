using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerPrefsSettings : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider volumeSlider;
    public Slider brightnessSlider;

    [Header("Post Processing")]
    public Volume globalVolume;
    private ColorAdjustments _colorAdjustments;

    [Header("Audio")]
    public AudioSource bgmSource;

    void Start()
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
        PlayerPrefs.SetFloat("MusicVolume", val);
        PlayerPrefs.Save();
        ApplyVolumeToMusic(val);
    }

    public void SaveBrightness()
    {
        float val = brightnessSlider.value;
        PlayerPrefs.SetFloat("Brightness", val);
        PlayerPrefs.Save();
        ApplyBrightnessToScreen(val);
    }

    private void ApplyVolumeToMusic(float value)
    {
        if (Time.timeScale < 0.1f)
            MusicController.Instance?.SetMuffled(true);
        else
            MusicController.Instance?.ResetEffects();

        if (bgmSource != null)
            bgmSource.volume = value;
    }

    private void ApplyBrightnessToScreen(float value)
    {
        if (_colorAdjustments != null)
            _colorAdjustments.postExposure.value = Mathf.Lerp(-3f, 3f, value);
    }
}