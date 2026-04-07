using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [Header("Movement")]
    [SerializeField] private float movingSpeed = 15f;
    private Rigidbody2D rb;
    private bool isRunning = false;
    private Vector2 moveDirection; // НОВАЯ ПЕРЕМЕННАЯ

    [Header("Weapon")]
    private bool hasWeapon = false;
    [SerializeField] private int maxAmmo = 30;
    private int currentAmmo;

    [Header("Shooting")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 0.15f;
    private float nextFireTime = 0f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI ammoText;

    private void Awake()
    {
        currentHealth = maxHealth;
        currentAmmo = maxAmmo;

        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        
    }

    private void Update()
    {
        if (Time.timeScale == 0) return;
        // Запоминаем направление движения
        if (GameInput.Instance != null)
        {
            moveDirection = GameInput.Instance.GetMovementVector().normalized;
        }
        
        // Стрельба
        if (hasWeapon && Mouse.current.leftButton.isPressed && Time.time >= nextFireTime && currentAmmo > 0)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
            currentAmmo--;
            UpdateAmmoUI();
            Debug.Log($"Ammo: {currentAmmo}/{maxAmmo}");
        }
        
        if (hasWeapon && Mouse.current.leftButton.isPressed && currentAmmo <= 0)
        {
            Debug.Log("Out of ammo!");
        }
    }

    private void FixedUpdate()
    {

        if (GameInput.Instance == null || rb == null) return;

        // 1. Поворот за мышью (Оставляем логику, но меняем метод на физический)
        Vector3 mousePos = GameInput.Instance.GetMousePosition();
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Используем MoveRotation, чтобы физика работала корректно
        rb.MoveRotation(angle);

        // 2. Движение (Исправлено: теперь WSAD всегда двигают вверх-вниз-влево-вправо)
        // Мы берем moveDirection напрямую и умножаем на скорость, не привязываясь к углу angle
        Vector2 moveVelocity = moveDirection.normalized * movingSpeed;

        // 3. Применяем движение
        rb.MovePosition(rb.position + moveVelocity * Time.fixedDeltaTime);

        isRunning = moveDirection.magnitude > 0.1f;
    }

    private void LateUpdate()
    {

        if (!hasWeapon) return;
        if (GameInput.Instance == null) return;
        if (firePoint == null) return;

        Vector3 mousePos = GameInput.Instance.GetMousePosition();
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void Shoot()
    {
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }

    private void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            // 1. Берем ссылку на фон
            GameObject background = ammoText.transform.parent.gameObject;

            // 2. Если оружие в руках — показываем фон и текст
            if (hasWeapon)
            {
                background.SetActive(true);
                ammoText.text = $"{currentAmmo}/{maxAmmo}";
                if(currentAmmo == 0)
                {
                    ammoText.text = $"No ammo!";
                }
            }
            else
            {
                // 3. Если оружия нет — скрываем всю панель целиком
                background.SetActive(false);
            }
        }
    }


    public void EquipWeapon()
    {
        hasWeapon = true;
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    public bool HasWeapon() => hasWeapon;
    public bool IsRunning() => isRunning;
    
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Player health: {currentHealth}");
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player died");
        gameObject.SetActive(false);
    }
}