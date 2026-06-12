using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI deathText;
    [SerializeField] private TextMeshProUGUI PtsText;
    public int currentPoints = 0;

    // Переменная для хранения ссылки на объект плашки
    private GameObject deathPanel;
    private bool isPlayerDead = false;
    private GameObject crosshair;

    public static RestartManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        if (MusicController.Instance != null)
        {
            MusicController.Instance.ResetEffects();
        }
        // Находим родителя (плашку) один раз при старте
        if (deathText != null)
        {
            deathPanel = deathText.transform.parent.gameObject;
            deathPanel.SetActive(false); // Выключаем при старте
        }
        currentPoints = 0;
        UpdatePtsUI();
    }

    public void AddPoints(int amount)
    {
        currentPoints += amount; // Прибавляем
        UpdatePtsUI(); // Обновляем текст
    }

    private void UpdatePtsUI()
    {
        if (PtsText != null)
        {
            PtsText.text = "PTS: " + currentPoints;
        }
    }

    public void ShowDeathScreen()
    {
        isPlayerDead = true;
        crosshair = GameObject.FindGameObjectWithTag("Crosshair");
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
            crosshair.SetActive(false);
        }

    }

    private void Update()
    {
        if (isPlayerDead && Input.GetKeyDown(KeyCode.R))
        {
            // Важно: если менял Time.timeScale, здесь его надо вернуть в 1
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
