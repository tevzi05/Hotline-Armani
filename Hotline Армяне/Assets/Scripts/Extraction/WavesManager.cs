using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct EnemySpawnConfig
{
    public GameObject enemyPrefab; // Префаб конкретного врага 
    public int count;              // Сколько штук заспавнить в волне
}

[System.Serializable]
public struct Wave
{
    public string waveName;
    public List<EnemySpawnConfig> enemiesToSpawn;
    public float spawnInterval;   // Интервал спавна
}

public class WavesManager : MonoBehaviour
{
    public static WavesManager Instance { get; private set; }

    private GameObject crosshair; // Курсор

    [Header("Настройки Спавна")]
    [SerializeField] private Transform[] spawnPoints; // Массив точек из ZombieSpawner
    [SerializeField] private List<Wave> waves;        // Максимум волн

    [Header("UI Элементы")]
    [SerializeField] private GameObject wavesContainer;
    [SerializeField] private TextMeshProUGUI wavesText;

    [Header("UI Элементы концовки")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private GameObject finalScoreContainer;

    [Header("UI Элементы победы")]
    [SerializeField] private TextMeshProUGUI vicWavesText;
    [SerializeField] private TextMeshProUGUI vicKillsText;
    [SerializeField] private TextMeshProUGUI vicTimeText;
    [SerializeField] private TextMeshProUGUI vicPointsText;
    [Header("UI Элементы поражения")]
    [SerializeField] private TextMeshProUGUI defWavesText;
    [SerializeField] private TextMeshProUGUI defKillsText;
    [SerializeField] private TextMeshProUGUI defTimeText;
    [SerializeField] private TextMeshProUGUI defPointsText;

    private int currentWaveIndex = 0;
    private int enemiesLeftToKill = 0;
    private bool isSurvivalStarted = false;

    // СТАТИСТИКА ДЛЯ ПОДВЕДЕНИЯ ИТОГОВ
    private float survivalTimer = 0f;
    private int totalKills = 0;
    private int completedWavesCount = 0;

    // Хэш-сет для отслеживания живых врагов
    private HashSet<GameObject> activeEnemies = new HashSet<GameObject>();

    
    private void Awake()
    {
        Instance = this;
        if (wavesContainer != null) wavesContainer.SetActive(false);
        if (finalScoreContainer != null) finalScoreContainer.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
    }

    private void Update()
    {
        if (isSurvivalStarted)
        {
            survivalTimer += Time.deltaTime;
        }
    }

    public void StartSurvivalMode()
    {
        if (isSurvivalStarted) return;

        isSurvivalStarted = true;
        currentWaveIndex = 0;
        survivalTimer = 0;
        totalKills = 0;
        completedWavesCount = 0;

        if (wavesContainer != null) wavesContainer.SetActive(true);

        StartNextWave();
    }

    private void StartNextWave()
    {
        if (currentWaveIndex >= waves.Count)
        {
            completedWavesCount = waves.Count; 
            EndSurvivalMode(true);
            return;
        }

        completedWavesCount = currentWaveIndex;
        StartCoroutine(SpawnWaveRoutine(waves[currentWaveIndex]));
    }

    private IEnumerator SpawnWaveRoutine(Wave wave)
    {
        // 1. Считаем врагов
        int totalEnemies = 0;
        for (int i = 0; i < wave.enemiesToSpawn.Count; i++)
        {
            totalEnemies += wave.enemiesToSpawn[i].count;
        }

        enemiesLeftToKill = totalEnemies;
        UpdateWaveUI();

        // Небольшая пауза перед стартом волны
        yield return new WaitForSeconds(2f);

        if (spawnPoints.Length == 0) yield break;

        // 2. Спавн противников
        for (int i = 0; i < wave.enemiesToSpawn.Count; i++)
        {
            EnemySpawnConfig config = wave.enemiesToSpawn[i];

            for (int k = 0; k < config.count; k++)
            {
                // Выбираем случайную точку
                Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];

                // Спавним нужного противника ровно в этой точке
                GameObject enemyObj = Instantiate(config.enemyPrefab, point.position, Quaternion.identity);

                // Добавляем в хэш-сет
                activeEnemies.Add(enemyObj);

                // Ждем интервал спавна, заданный для этой волны
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }
    }

    // Этот метод вызывается из Zombie.cs при смерти
    public void RegisterEnemyDeath(GameObject enemy)
    {
        if (!isSurvivalStarted) return;

        
        if (activeEnemies.Remove(enemy))
        {
            enemiesLeftToKill--;
            totalKills++;
            if (enemiesLeftToKill < 0) enemiesLeftToKill = 0;

            UpdateWaveUI();

            // Если территория зачищена — мгновенно переходим к следующей волне
            if (enemiesLeftToKill <= 0)
            {
                currentWaveIndex++;
                StartNextWave();
            }
        }
    }

    private void UpdateWaveUI()
    {
        if (wavesText == null) return;
        wavesText.text = $"WAVE {currentWaveIndex + 1}/{waves.Count} | ENEMIES: {enemiesLeftToKill}";
    }

    private void EndSurvivalMode(bool isVictory)
    {
        isSurvivalStarted = false;
        if (wavesContainer != null) wavesContainer.SetActive(false);

        int finalPoints = 0;
        if (RestartManager.Instance != null)
        {

            finalPoints = RestartManager.Instance.currentPoints; 
        }
        int minutes = Mathf.FloorToInt(survivalTimer / 60f);
        int seconds = Mathf.FloorToInt(survivalTimer % 60f);
        string timeString = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (finalScoreContainer != null) finalScoreContainer.SetActive(true);
        if (isVictory)
        {
            if (victoryPanel != null) victoryPanel.SetActive(true);
            if (defeatPanel != null) defeatPanel.SetActive(false); // На всякий случай выключаем поражение

            // Заполняем тексты ПОБЕДЫ
            if (vicWavesText != null) vicWavesText.text = $"WAVES COMPLETED: {completedWavesCount}/{waves.Count}";
            if (vicKillsText != null) vicKillsText.text = $"ENEMIES DEFEATED: {totalKills}";
            if (vicTimeText != null) vicTimeText.text = $"TIME: {timeString}";
            if (vicPointsText != null) vicPointsText.text = $"TOTAL POINTS: {finalPoints}";
        }
        else
        {
            if (victoryPanel != null) victoryPanel.SetActive(false); // На всякий случай выключаем победу
            if (defeatPanel != null) defeatPanel.SetActive(true);

            // Заполняем тексты ПОРАЖЕНИЯ
            if (defWavesText != null) defWavesText.text = $"WAVES COMPLETED: {completedWavesCount}/{waves.Count}";
            if (defKillsText != null) defKillsText.text = $"ENEMIES DEFEATED: {totalKills}";
            if (defTimeText != null) defTimeText.text = $"TIME: {timeString}";
            if (defPointsText != null) defPointsText.text = $"TOTAL POINTS: {finalPoints}";
        }
        Time.timeScale = 0f;
        crosshair = GameObject.FindGameObjectWithTag("Crosshair");
        if (crosshair != null) crosshair.SetActive(false);
        Cursor.visible = true;
        
    }
}
