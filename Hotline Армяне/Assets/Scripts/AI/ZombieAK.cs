using UnityEngine;
using Pathfinding; // Для AIDestinationSetter и AIPath
using System.Collections;

public class ZombieAK : Zombie // Наследуемся от базового Zombie
{
    [Header("Ranged Attack (AK)")]
    [SerializeField] private WeaponData weaponData;       // ScriptableObject автомата
    [SerializeField] private Transform firePoint;         // Точка стрельбы
    [SerializeField] private float shootingDistance = 15f; // Дистанция, на которой он останавливается и стреляет
    [SerializeField] private float enemyReloadTime = 1.5f;
    private Weapon enemyWeapon;
    private float nextFireTime = 0f;
    private bool isReloading = false;

    protected override void Start()
    {
        base.Start();


        if (firePoint != null)
        {
            enemyWeapon = firePoint.GetComponent<Weapon>();


            if (enemyWeapon == null)
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

            // Запускаем логику дистанции и стрельбы
            HandleDistanceAndShooting();
        }
        else
        {
            // Если игрока не видим — смотрим по направлению движения
            if (ai != null && ai.velocity.sqrMagnitude > 0.1f)
            {
                lookDirection = ai.velocity;
            }
            else
            {
                lookDirection = transform.right;
            }

            // Если потеряли из виду — принудительно заставляем бежать по коридору дальше
            if (ai != null) ai.isStopped = false;
        }

        // Поворачиваем зомби (rotationOffset подтянется из инспектора базового Zombie)
        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        rb.MoveRotation(angle + rotationOffset);
    }

    private void HandleDistanceAndShooting()
    {
        if (enemyWeapon == null || isReloading || player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= shootingDistance)
        {
            // Тормозим навигацию, чтобы он стоял и стрелял
            if (ai != null) ai.isStopped = true;

            // Стреляем
            HandleEnemyShooting();
        }
        else
        {
            // Если игрок далеко — бежим за ним дальше
            if (ai != null) ai.isStopped = false;
        }
    }

    private void HandleEnemyShooting()
    {
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            if (ai != null) ai.isStopped = false;
            return;
        }
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

        yield return new WaitForSeconds(enemyReloadTime); // Время перезарядки
        enemyWeapon.ForceInstantReload();
        if (ai != null) ai.isStopped = false;
        isReloading = false;
    }
}