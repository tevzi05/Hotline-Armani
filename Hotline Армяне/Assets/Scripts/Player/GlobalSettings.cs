using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Audio;

public class GlobalSettings : MonoBehaviour
{
    public static GlobalSettings Instance; // Позволит обращаться к скрипту из любой сцены

    [Header("Links")]
    private AudioSource bgmSource;
    private ColorAdjustments _colorAdjustments;

    [Header("AudioMixer")]
    [SerializeField] private AudioMixer mainMixer;
    private const string MIXER_SFX = "SFXVolume";

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
    public void InitForNewScene(Slider bSlider, Slider vSlider, Slider sfxSlider, Volume sceneVolume)
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
        float sfxVal = PlayerPrefs.GetFloat("SFXVolume", 0.5f);

        // Применяем
        ApplyBrightness(bVal);
        ApplyVolume(vVal);
        ApplySFXVolume(sfxVal);

        // Связываем ползунки, если они есть в этой сцене
        if (bSlider != null) bSlider.value = bVal;
        if (vSlider != null) vSlider.value = vVal;
        if (sfxSlider != null) sfxSlider.value = sfxVal;
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

    public void ApplySFXVolume(float val)
    {
        PlayerPrefs.SetFloat("SFXVolume", val);

        if (mainMixer != null)
        {
            if (val <= 0.0001f)
            {
                mainMixer.SetFloat(MIXER_SFX, -80f); // -80 дБ — это полная тишина в Unity
            }
            else
            {
                // Масштабируем от -40 дБ до 0 дБ
                float dB = Mathf.Log10(val) * 20;

                // Ограничиваем децибелы, чтобы они не улетали ниже -80 дБ в любом случае
                dB = Mathf.Clamp(dB, -80f, 0f);

                mainMixer.SetFloat(MIXER_SFX, dB);
            }
        }
    }


    private void ApplyAllSettings()
    {
        float bVal = PlayerPrefs.GetFloat("Brightness", 1.0f);
        float vVal = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        float sfxVal = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        ApplyBrightness(bVal);
        ApplyVolume(vVal);
        ApplySFXVolume(sfxVal);
    }
}
