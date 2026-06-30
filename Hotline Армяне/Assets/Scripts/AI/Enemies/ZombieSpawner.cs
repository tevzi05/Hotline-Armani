using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    public GameObject zombiePrefab;
    public float spawnInterval = 2f;

    [Header("Точки спавна")]
    public Transform[] spawnPoints;

    private float timer;

    private void Start()
    {
        timer = spawnInterval;
    }

    private void Update()
    {
        if (spawnPoints.Length == 0) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            SpawnAtPoint();
            timer = spawnInterval;
        }
    }

    private void SpawnAtPoint()
    {
        // Выбираем случайную точку в списке
        int index = Random.Range(0, spawnPoints.Length);
        Transform point = spawnPoints[index];

        // Создаем зомби ровно в этой точке
        Instantiate(zombiePrefab, point.position, Quaternion.identity);
        
    }
}
