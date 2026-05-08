using UnityEngine;

public class MusicController : MonoBehaviour
{
    public static MusicController Instance;
    private AudioSource audioSource;
    private float defaultVolume;

    private float CurrentConfigVolume => PlayerPrefs.GetFloat("MusicVolume", 0.5f);
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.volume = CurrentConfigVolume;
        //defaultVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f); ; // Запоминаем громкость из инспектора
    }

    // Возвращает музыку в нормальное состояние
    public void ResetEffects()
    {
        audioSource.pitch = 1f;
        audioSource.volume = CurrentConfigVolume; // Берем из конфига, а не из старой переменной
        //audioSource.volume = defaultVolume;
    }

    // Полный перезапуск трека
    public void RestartMusic()
    {
        audioSource.Stop();
        ResetEffects();
        audioSource.Play();
        //ResetEffects();
        //audioSource.Stop();
        //audioSource.Play();
    }
    public void StopMusic()
    {
        audioSource.Stop();
    }
    public void StartMusic()
    {
        ResetEffects();
        audioSource.Play();
    }

    // Метод для приглушения (вызывай из Паузы или при Смерти)
    public void SetMuffled(bool muffled)
    {
        audioSource.pitch = muffled ? 0.6f : 1f;
        // Приглушаем относительно АКТУАЛЬНОЙ громкости
        audioSource.volume = muffled ? CurrentConfigVolume * 0.3f : CurrentConfigVolume;
        //audioSource.volume = muffled ? defaultVolume * 0.3f : defaultVolume;
    }
}
