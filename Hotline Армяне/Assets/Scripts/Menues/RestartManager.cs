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

    // Переменные для чекпоинтов
    private Vector3 lastCheckpointPosition;
    private bool hasCheckpoint = false;
    private GameObject player;

    public static RestartManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        if (MusicController.Instance != null) MusicController.Instance.ResetEffects();
        if (deathText != null)
        {
            deathPanel = deathText.transform.parent.gameObject;
            deathPanel.SetActive(false); 
        }
        currentPoints = 0;
        UpdatePtsUI();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void AddPoints(int amount)
    {
        currentPoints += amount; // Прибавляем
        UpdatePtsUI(); // Обновляем текст
    }

    public void SetCheckpoint(Vector3 position)
    {
        lastCheckpointPosition = position;
        hasCheckpoint = true;
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
            if (hasCheckpoint && player != null)
            {
                RespawnAtCheckpoint();
            }
            else
            {
                // Если чекпоинта еще не было, перезапускаем сцену полностью
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    private void RespawnAtCheckpoint()
    {
        isPlayerDead = false;

        // Перемещаемся на точку чекпоинта
        player.transform.position = lastCheckpointPosition;

        if (deathPanel != null) deathPanel.SetActive(false);
        if (crosshair != null) crosshair.SetActive(true);
        if (MusicController.Instance != null) MusicController.Instance.ResetEffects();

    }
}
