using UnityEngine;
using Pathfinding; // Для AIDestinationSetter и AIPath
using System.Collections;

public class ZombiePistol : Zombie
{

    [Header("Ranged Attack (Pistol)")]
    [SerializeField] private WeaponData weaponData;       // ScriptableObject пистолета
    [SerializeField] private Transform firePoint;         // Точка стрельбы
    [SerializeField] private float shootingDistance = 15f; // Дистанция, на которой он останавливается и стреляет
    private Weapon enemyWeapon;
    private float nextFireTime = 0.4f;
    private bool isReloading = false;


    protected override void Start()
    {
        base.Start();
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

    protected override void FixedUpdate()
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
}
