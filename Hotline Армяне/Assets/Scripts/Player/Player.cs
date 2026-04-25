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
    private Vector2 moveDirection;

    [Header("Weapon System")]
    [SerializeField] private WeaponData currentWeaponData; // Ссылка на текущие настройки оружия
    [SerializeField] private AudioSource weaponAudioSource;
    private bool hasWeapon = false;
    private int currentAmmo;
    private float nextFireTime = 0f;

    [Header("Setup")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab; // Можно оставить как дефолт, если оружия нет

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI ammoText;

    private void Awake()
    {
        currentHealth = maxHealth;
        Instance = this;
        rb = GetComponent<Rigidbody2D>();

        if (weaponAudioSource == null)
            weaponAudioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Time.timeScale == 0) return;

        // Ввод движения
        if (GameInput.Instance != null)
            moveDirection = GameInput.Instance.GetMovementVector().normalized;

        // Логика стрельбы
        HandleShooting();
    }

    private void HandleShooting()
    {
        if (!hasWeapon || currentWeaponData == null) return;

        bool isFiring = Mouse.current.leftButton.isPressed;

        if (isFiring && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
            {
                Shoot();
            }
            else
            {
                PlayEmptySound();
                nextFireTime = Time.time + 0.25f;
            }
        }
    }
    private void PlayEmptySound()
    {
        if (currentWeaponData.emptySound != null && !weaponAudioSource.isPlaying)
        {
            weaponAudioSource.PlayOneShot(currentWeaponData.emptySound);
        }
    }

    private void Shoot()
    {
        // Создаем пулю (префаб берем из данных оружия)
        Instantiate(currentWeaponData.bulletPrefab, firePoint.position, firePoint.rotation);

        // Звук (берем из данных оружия)
        if (currentWeaponData.shootSound != null)
        {
            weaponAudioSource.pitch = Random.Range(currentWeaponData.minPitch, currentWeaponData.maxPitch);
            weaponAudioSource.PlayOneShot(currentWeaponData.shootSound);
        }

        // Расход ресурсов
        currentAmmo--;
        nextFireTime = Time.time + currentWeaponData.fireRate;
        UpdateAmmoUI();
    }


    // Этот метод вызывается, когда игрок наступает на предмет оружия
    public void EquipWeapon(WeaponData newData)
    {
        currentWeaponData = newData;
        hasWeapon = true;
        currentAmmo = newData.maxAmmo;

        // СМЕНА АНИМАЦИЙ:
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null && newData.weaponOverride != null)
        {
            anim.runtimeAnimatorController = newData.weaponOverride;
        }

        if (weaponAudioSource != null && newData.weaponPickup != null)
        {
            weaponAudioSource.pitch = 1f;
            weaponAudioSource.PlayOneShot(newData.weaponPickup);
        }
        UpdateAmmoUI();
    }


    //ДОБАВЛЕНО
    public void AddAmmo(int amount)
    {
        if (!hasWeapon || currentWeaponData == null) return;

        int newAmmo = currentAmmo + amount;
        currentAmmo = Mathf.Min(newAmmo, currentWeaponData.maxAmmo);
        UpdateAmmoUI();

        Debug.Log($"+{amount} ammo. Now: {currentAmmo}/{currentWeaponData.maxAmmo}");
    }
    //ЧТО НИЖЕ УЖЕ БЫЛО

    private void FixedUpdate()
    {
        if (GameInput.Instance == null || rb == null) return;

        // Поворот
        Vector3 mousePos = GameInput.Instance.GetMousePosition();
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.MoveRotation(angle);

        // Движение
        Vector2 moveVelocity = moveDirection * movingSpeed;
        rb.MovePosition(rb.position + moveVelocity * Time.fixedDeltaTime);

        isRunning = moveDirection.magnitude > 0.1f;
    }

    private void LateUpdate()
    {
        if (!hasWeapon || firePoint == null) return;

        Vector3 mousePos = GameInput.Instance.GetMousePosition();
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void UpdateAmmoUI()
    {
        if (ammoText == null) return;

        GameObject background = ammoText.transform.parent.gameObject;

        if (hasWeapon)
        {
            background.SetActive(true);
            ammoText.text = currentAmmo > 0 ? $"{currentAmmo}/{currentWeaponData.maxAmmo}" : "No ammo!";
        }
        else
        {
            background.SetActive(false);
        }
    }

    public bool HasWeapon() => hasWeapon;
    public bool IsRunning() => isRunning;

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        gameObject.SetActive(false);
    }
}
