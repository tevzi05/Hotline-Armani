using UnityEngine;

public class WaveMusicController : MonoBehaviour
{
    public static WaveMusicController Instance;
    private AudioSource audioSource;

    [Header("Музыкальные треки")]
    [SerializeField] private AudioClip openingTrack; // Трек до разговора с NPC
    [SerializeField] private AudioClip[] waveTracks;   // Массив треков для волн

    [Header("Настройки рандома")]
    [SerializeField] private bool useRandomMusic = false; // Включить рандом для волн?

    private float CurrentConfigVolume => PlayerPrefs.GetFloat("MusicVolume", 0.5f);

    private void Awake()
    {
        if (MusicController.Instance != null)
        {
            MusicController.Instance.StopMusic();
        }

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        audioSource = GetComponent<AudioSource>();
        audioSource.volume = CurrentConfigVolume;

    }

    private void Start()
    {

        PlayTrack(openingTrack);
    }

    // Метод включения конкретного трека
    private void PlayTrack(AudioClip clip)
    {
        if (clip == null) return;

        audioSource.Stop();
        audioSource.clip = clip;
        ResetEffects();
        audioSource.Play();
    }

    // Этот метод вызывай из своего скрипта волн, когда начинается НОВАЯ волна
    public void StartNextWaveMusic(int waveNumber)
    {
        // Если треков для волн нет, ничего не делаем
        if (waveTracks == null || waveTracks.Length == 0) return;

        if (useRandomMusic)
        {
            // Будущая логика рандома: выбираем случайный трек из массива
            int randomIndex = Random.Range(0, waveTracks.Length);
            PlayTrack(waveTracks[randomIndex]);
        }
        else
        {
            // Фиксированная логика: waveNumber (1, 2, 3...) сопоставляем с индексом массива (0, 1, 2...)
            // % waveTracks.Length нужен, чтобы если волн больше, чем треков, музыка пошла по кругу
            int trackIndex = (waveNumber - 1) % waveTracks.Length;
            PlayTrack(waveTracks[trackIndex]);
        }
    }

    public void ResetEffects()
    {
        audioSource.pitch = 1f;
        audioSource.volume = CurrentConfigVolume;
    }

    public void SetMuffled(bool muffled)
    {
        audioSource.pitch = muffled ? 0.6f : 1f;
        audioSource.volume = muffled ? CurrentConfigVolume * 0.3f : CurrentConfigVolume;
    }
}