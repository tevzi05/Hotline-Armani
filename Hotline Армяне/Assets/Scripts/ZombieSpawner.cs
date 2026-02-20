using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    public GameObject zombiePrefab;
    public float spawnInterval = 2f;
    public float spawnRadius = 5f;

    private Transform player;
    private float timer;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        timer = spawnInterval;
    }

    private void Update()
    {
        if (player == null) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = player.position + new Vector3(offset.x, offset.y, 0);
            Instantiate(zombiePrefab, spawnPos, Quaternion.identity);
            timer = spawnInterval;
        }
    }
}