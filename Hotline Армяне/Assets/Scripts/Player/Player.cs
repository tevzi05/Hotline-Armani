using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using UnityEngine.Audio;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [Header("Movement")]
    [SerializeField] private float movingSpeed = 15f;
    private Rigidbody2D rb;
    private bool isRunning = false;
    private Vector2 moveDirection;

    [Header("Weapon System")]
    private Weapon currentWeapon; // Ссылка на новый чистый компонент оружия
    private bool hasWeapon = false;
    private float nextFireTime = 0f;
    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("Reloading")]
    public float reloadTime = 1.5f;
    private bool isReloading = false;

    [Header("Setup")]
    [SerializeField] private Transform firePoint;

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
    }

    private void Update()
    {
        if (Time.timeScale == 0) return;

        // Ввод движения
        if (GameInput.Instance != null)
            moveDirection = GameInput.Instance.GetMovementVector().normalized;

        // Логика стрельбы
        HandleShooting();

        // Логика перезарядки (проверяем условия теперь через скрипт Weapon)
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && hasWeapon && currentWeapon != null)
        {
            if (currentWeapon.NeedsReload())
            {
                StartCoroutine(Reload());
            }
        }
    }

    private void HandleShooting()
    {
        // Если оружия нет, или таймер стрельбы еще не прошел — ничего не делаем
        if (!hasWeapon || currentWeapon == null || !currentWeapon.CanShoot(nextFireTime)) return;

        bool isFiring = Mouse.current.leftButton.isPressed;

        if (isFiring)
        {
            // Передаем управление выстрелом самой пушке.
            // Она сама создаст пулю, заберет патрон, сыграет нужный звук и вернет задержку (fireRate).
            float fireDelay = currentWeapon.Fire(firePoint);

            // Задаем время следующего выстрела
            nextFireTime = Time.time + fireDelay;

            UpdateAmmoUI();
        }
    }

    public void EquipWeapon(WeaponData newData)
    {
        if (newData == null) return;

        currentWeapon = firePoint.GetComponent<Weapon>();

        if (currentWeapon != null)
        {
            currentWeapon.Init(newData);
            hasWeapon = true;
        }
        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null && newData.weaponOverride != null)
        {
            anim.runtimeAnimatorController = newData.weaponOverride;
        }

        UpdateAmmoUI();
    }

    public void AddAmmo()
    {
        if (!hasWeapon || currentWeapon == null) return;

        // Делегируем задачу добавления патронов пушке
        currentWeapon.AddAmmo();
        UpdateAmmoUI();
    }

    private IEnumerator Reload()
    {
        isReloading = true;

        yield return new WaitForSeconds(reloadTime);

        if (currentWeapon != null)
        {
            // Пушка сама пересчитает свои патроны в магазине и запасе
            currentWeapon.ExecuteReload();
        }

        UpdateAmmoUI();
        isReloading = false;
    }

    private void FixedUpdate()
    {
        if (GameInput.Instance == null || rb == null) return;

        // Поворот (твой код)
        Vector3 mousePos = GameInput.Instance.GetMousePosition();
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.MoveRotation(angle);

        // Движение (твой код)
        Vector2 moveVelocity = moveDirection * movingSpeed;
        rb.MovePosition(rb.position + moveVelocity * Time.fixedDeltaTime);

        isRunning = moveDirection.magnitude > 0.1f;
    }

    private void LateUpdate()
    {
        if (!hasWeapon || firePoint == null) return;

        // Поворот точки стрельбы за мышкой (твой код)
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

        if (hasWeapon && currentWeapon != null)
        {
            background.SetActive(true);

            // Запрашиваем красивый текст патронов у самой пушки
            ammoText.text = currentWeapon.GetAmmoText();

            // Проверяем, кончились ли патроны совсем
            if (currentWeapon.IsOutofAmmo())
                ammoText.text = "No ammo!";

            // Включаем плашку перезарядки, если магазин пуст
            backgroundReload.SetActive(currentWeapon.IsMagazineEmpty());
        }
        else
        {
            background.SetActive(false);
            backgroundReload.SetActive(false);
        }
    }

    public bool HasWeapon() => hasWeapon;
    public bool IsDead() => currentHealth <= 0;
    public bool IsRunning() => isRunning;

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        if (RestartManager.Instance != null)
        {
            RestartManager.Instance.ShowDeathScreen();
        }
        gameObject.SetActive(false);
    }
    
}
