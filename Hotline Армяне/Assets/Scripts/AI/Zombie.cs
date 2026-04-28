using UnityEngine;
// Добавь эту строку сверху, чтобы скрипт понимал компоненты навигации
using Pathfinding;

public class Zombie : MonoBehaviour
{

    [Header("Health")]
    public int maxHealth = 1;
    private int currentHealth;

    [Header("Attack")]
    public int damageToPlayer = 20;
    public float attackCooldown = 1f;
    private float lastAttackTime;

    [Header("Points")]
    public int points = 100;

    private Transform player;

    private void Start()
    {
        currentHealth = maxHealth;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            // Принудительно отдаем трансформ игрока в навигацию клона
            GetComponent<AIDestinationSetter>().target = playerObj.transform;
        }
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Автоматически находим игрока и подставляем его в навигацию
        var setter = GetComponent<AIDestinationSetter>();
        if (setter != null && player != null)
        {
            setter.target = player;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && Time.time > lastAttackTime + attackCooldown)
        {
            Player playerHealth = other.GetComponent<Player>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageToPlayer);
                lastAttackTime = Time.time;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            if (RestartManager.Instance != null)
            {
                RestartManager.Instance.AddPoints(100);
            }
            Destroy(gameObject);
        }
    }
}