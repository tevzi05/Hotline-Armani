using UnityEngine;
using Pathfinding; // Для AIDestinationSetter и AIPath
using System.Collections;

public class ZombieAK : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 1; 
    private int currentHealth;

    [Header("Attack (Melee)")]
    public int damageToPlayer = 20;
    public float attackCooldown = 1f;
    private float lastAttackTime;

    [Header("Ranged Attack (AK)")]
    [SerializeField] private WeaponData weaponData;       // ScriptableObject автомата
    [SerializeField] private Transform firePoint;         // Точка стрельбы
    [SerializeField] private float shootingDistance = 15f; // Дистанция, на которой он останавливается и стреляет
    private Weapon enemyWeapon;
    private float nextFireTime = 0f;
    private bool isReloading = false;

    [Header("Points")]
    public int points = 200;

    [Header("Rotation & FOV")]
    public float rotationOffset = 0f;
    public float viewDistance = 100f;     // Дистанция зрения
    public LayerMask obstacleMask;       // Слой стен

    private Transform player;
    private Rigidbody2D rb;
    private IAstarAI ai;

    private void Start()
    {
        currentHealth = maxHealth;
        ai = GetComponent<IAstarAI>();
        rb = GetComponent<Rigidbody2D>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;

            // Навигация
            var setter = GetComponent<AIDestinationSetter>();
            if (setter != null) setter.target = player;
        }

        // Инициализируем оружие
        enemyWeapon = GetComponentInChildren<Weapon>();
        if (enemyWeapon == null && firePoint != null)
        {
            enemyWeapon = firePoint.gameObject.AddComponent<Weapon>();
        }

        if (enemyWeapon != null && weaponData != null)
        {
            enemyWeapon.Init(weaponData);
        }
    }

    private void FixedUpdate()
    {
        if (rb == null || player == null) return;

        Vector2 lookDirection;

        if (CanSeePlayer())
        {
            // Смотрим прямо на игрока
            lookDirection = (Vector2)player.position - rb.position;
        }
        else if (ai != null && ai.velocity.sqrMagnitude > 0.1f)
        {
            // Смотрим по направлению движения
            lookDirection = ai.velocity;
        }
        else
        {
            return;
        }

        // Поворачиваем зомби
        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        rb.MoveRotation(angle + rotationOffset);

        // Логика стрельбы и остановки
        HandleDistanceAndShooting();
    }

    private void HandleDistanceAndShooting()
    {
        if (enemyWeapon == null || isReloading || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Проверяем: видим ли игрока и подошли ли на дистанцию выстрела
        if (CanSeePlayer() && distanceToPlayer <= shootingDistance)
        {
            // Тормозим навигацию, чтобы он стоял и стрелял
            if (ai != null) ai.isStopped = true;

            // Стреляем
            HandleEnemyShooting();
        }
        else
        {
            // Если потеряли или игрок далеко — бежим за ним дальше
            if (ai != null) ai.isStopped = false;
        }
    }

    private void HandleEnemyShooting()
    {
        if (!enemyWeapon.CanShoot(nextFireTime)) return;
        Player playerScript = player.GetComponent<Player>();
        if (playerScript != null && playerScript.IsDead())
        {
            if (ai != null) ai.isStopped = false; 
            return;
        }

        // Перезарядка, если кончились патроны
        if (enemyWeapon.IsMagazineEmpty())
        {
            if (enemyWeapon.NeedsReload())
            {
                StartCoroutine(EnemyReload());
            }
            return;
        }
        float fireDelay = enemyWeapon.Fire(firePoint);
        nextFireTime = Time.time + fireDelay + Random.Range(0f, 0.05f);
    }

    private IEnumerator EnemyReload()
    {
        isReloading = true;
        if (ai != null) ai.isStopped = true;

        yield return new WaitForSeconds(1.5f); // Время перезарядки

        if (enemyWeapon != null)
        {
            enemyWeapon.ExecuteReload();
        }

        isReloading = false;
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer > viewDistance) return false;

        RaycastHit2D hit = Physics2D.Linecast(transform.position, player.position, obstacleMask);
        return hit.collider == null;
    }

    // Если игрок подошел вплотную, зомби все еще может ударить его руками
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

    private void Die()
    {
        if (ai != null) ai.isStopped = false;
        if (RestartManager.Instance != null) RestartManager.Instance.AddPoints(points);

        Player playerScript = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
        if (playerScript != null)
        {
            playerScript.AddAmmo();
        }
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, shootingDistance); // Оранжевый радиус для дистанции стрельбы
    }
}
