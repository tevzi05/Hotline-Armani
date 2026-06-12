using UnityEngine;
using Pathfinding; // Для AIDestinationSetter и AIPath

public class Zombie : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 1;
    protected int currentHealth;


    [Header("Attack")]
    public int damageToPlayer = 20;
    public float attackCooldown = 1f;
    protected float lastAttackTime;

    [Header("Points")]
    public int points = 100;

    [Header("Rotation & FOV")]
    public float rotationOffset = 0f;
    public float viewDistance = 100f;     // Дистанция зрения
    public LayerMask obstacleMask;       // Слой стен (выбери в инспекторе)

    protected Transform player;
    protected Rigidbody2D rb;
    protected IAstarAI ai;
    

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        ai = GetComponent<IAstarAI>();
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;

            // Навигация (будет работать всегда)
            var setter = GetComponent<AIDestinationSetter>();
            if (setter != null) setter.target = player;
        }
    }

    protected virtual void FixedUpdate()
    {
        if (rb == null || player == null) return;

        Vector2 lookDirection;

        if (CanSeePlayer())
        {
            // 1. Если видим игрока — смотрим прямо на него
            lookDirection = (Vector2)player.position - rb.position;
        }
        else if (ai != null && ai.velocity.sqrMagnitude > 0.1f)
        {
            // 2. Если НЕ видим, но движемся — смотрим по направлению движения (по вектору пути)
            lookDirection = ai.velocity;
        }
        else
        {
            // 3. Если стоим и не видим — ничего не меняем
            return;
        }

        // Поворачиваем зомби в сторону выбранного вектора
        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        rb.MoveRotation(angle + rotationOffset);
    }

    protected bool CanSeePlayer()
    {
        // 1. Проверяем дистанцию
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer > viewDistance) return false;

        // 2. Проверяем препятствия (стены)
        // Пускаем луч от зомби к игроку. Если на пути стена (obstacleMask) — не видим.
        RaycastHit2D hit = Physics2D.Linecast(transform.position, player.position, obstacleMask);

        if (hit.collider == null)
        {
            return true; // Препятствий нет
        }

        return false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && Time.time > lastAttackTime + attackCooldown)
        {
            var playerHealth = other.GetComponent<Player>();
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
        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        if (RestartManager.Instance != null) RestartManager.Instance.AddPoints(points);
        Player playerScript = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();

        if (WavesManager.Instance != null)
        {
            WavesManager.Instance.RegisterEnemyDeath(this.gameObject);
        }
        if (playerScript != null)
        {
            playerScript.AddAmmo();
        }
        Destroy(gameObject);
    }

    // Рисует радиус зрения в редакторе Unity
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewDistance);
    }
}
