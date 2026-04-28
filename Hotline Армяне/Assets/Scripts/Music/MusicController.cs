using UnityEngine;

public class MusicController : MonoBehaviour
{
    public static MusicController Instance;
    private AudioSource audioSource;
    private float defaultVolume;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        defaultVolume = audioSource.volume; // Запоминаем громкость из инспектора
    }

    // Возвращает музыку в нормальное состояние
    public void ResetEffects()
    {
        audioSource.pitch = 1f;
        audioSource.volume = defaultVolume;
    }

    // Полный перезапуск трека
    public void RestartMusic()
    {
        ResetEffects();
        audioSource.Stop();
        audioSource.Play();
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
        audioSource.volume = muffled ? defaultVolume * 0.3f : defaultVolume;
    }
}
