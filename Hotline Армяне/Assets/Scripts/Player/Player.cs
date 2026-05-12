using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

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
    private int ammoReserve;      // Патроны в запасе (кармане)
    private int magazineSize;
    private float nextFireTime = 0f;

    [Header("Reloading")]
    public float reloadTime = 1.5f;
    private bool isReloading = false;

    [Header("Setup")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab; // Можно оставить как дефолт, если оружия нет

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI reloadText;


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
        if (GameInput.Instance != null) moveDirection = GameInput.Instance.GetMovementVector().normalized;

        // Логика стрельбы
        HandleShooting();
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < magazineSize && ammoReserve > 0)
        {
            StartCoroutine(Reload());
        }

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
        if (isReloading || currentAmmo <= 0) return;
        currentAmmo--;
        nextFireTime = Time.time + currentWeaponData.fireRate;
        UpdateAmmoUI();
    }



    public void EquipWeapon(WeaponData newData)
    {
        currentWeaponData = newData;
        hasWeapon = true;
        currentAmmo = newData.maxAmmo;
        magazineSize = newData.maxAmmo;
        ammoReserve = newData.maxAmmo;

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

    public void AddAmmo()
    {
        if (!hasWeapon || currentWeaponData == null) return;
        int amount = currentWeaponData.ammoPerKill;
        ammoReserve += amount;
        UpdateAmmoUI();

        Debug.Log($"+{amount} ammo. Now: {currentAmmo}/{ammoReserve}");
    }

    private IEnumerator Reload()
    {
        isReloading = true;

        yield return new WaitForSeconds(reloadTime);

        /// Считаем, сколько пуль не хватает в магазине
        int amountNeeded = magazineSize - currentAmmo;

        // Проверяем, хватает ли нам патронов в запасе
        int amountToTake = Mathf.Min(amountNeeded, ammoReserve);

        // Перекладываем из запаса в магазин
        currentAmmo += amountToTake;
        ammoReserve -= amountToTake;

        UpdateAmmoUI();
        isReloading = false;
    }

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
        GameObject backgroundReload = reloadText.transform.parent.gameObject;

        if (hasWeapon)
        {
            background.SetActive(true);
            if (ammoReserve >= 0) ammoText.text = $"{currentAmmo}/{ammoReserve}";
            if (ammoReserve == 0 && currentAmmo == 0) ammoText.text = "No ammo!";
            if (currentAmmo == 0) backgroundReload.SetActive(true);
            else backgroundReload.SetActive(false);
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
        // Оповещаем менеджер
        if (RestartManager.Instance != null)
        {
            RestartManager.Instance.ShowDeathScreen();
        }

        // Выключаем игрока
        gameObject.SetActive(false);
    }

}
