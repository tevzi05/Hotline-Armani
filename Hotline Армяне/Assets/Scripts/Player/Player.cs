using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using UnityEngine.Audio;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [Header("Dialogue Lock")]
    private bool isLockedInDialogue = false;

    [Header("Movement")]
    [SerializeField] private float movingSpeed = 15f;
    private Rigidbody2D rb;
    private bool isRunning = false;
    private Vector2 moveDirection;

    [Header("Weapon System")]
    private Weapon currentWeapon;
    private bool hasWeapon = false;
    private float nextFireTime = 0f;
    [SerializeField] private AudioMixerGroup sfxGroup;

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

        if (isLockedInDialogue) return;

        // Ввод движения
        if (GameInput.Instance != null)
            moveDirection = GameInput.Instance.GetMovementVector().normalized;

        // Логика стрельбы
        HandleShooting();

        // Логика перезарядки
        if (Input.GetKeyDown(KeyCode.R) && currentWeapon != null)
        {
            if (currentWeapon.NeedsReload())
            {
                currentWeapon.StartReload(UpdateAmmoUI);
            }
        }
    }
    // СТРЭЛЬБА
    private void HandleShooting()
    {

        if (!hasWeapon || currentWeapon == null || !currentWeapon.CanShoot(nextFireTime)) return;

        bool isFiring = Mouse.current.leftButton.isPressed;

        if (isFiring)
        {
            // ЕСЛИ ИДЕТ ПЕРЕЗАРЯДКА:
            if (currentWeapon.IsReloading)
            {
                // Если это дробовик и в нем уже есть патроны — прерываем перезарядку для выстрела!
                if (!currentWeapon.IsMagazineEmpty())
                {
                    currentWeapon.TryInterruptReload();
                }
                return;
            }


            if (!currentWeapon.CanShoot(nextFireTime)) return;

            // Сам выстрел
            float fireDelay = currentWeapon.Fire(firePoint);
            nextFireTime = Time.time + fireDelay;
            UpdateAmmoUI();
        }
    }

    // ЭКИПИРОВКА ОРУЖИЯ
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

    // ДОБАВЛЕНИЕ ПАТРОНОВ
    public void AddAmmo()
    {
        if (!hasWeapon || currentWeapon == null) return;

        // Делегируем задачу добавления патронов пушке
        currentWeapon.AddAmmo();
        Debug.Log("Добавил патроны");
        UpdateAmmoUI();
    }

    // ОБНОВЛЕНИЕ ДВИЖЕНИЙ
    private void FixedUpdate()
    {
        if (GameInput.Instance == null || rb == null) return;

        if (isLockedInDialogue) return;

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

    // ПОВОРОТ ТОЧКИ СТРЕЛЬБЫ В СТОРОНУ МЫШКИ
    private void LateUpdate()
    {

        if (isLockedInDialogue) return;

        if (!hasWeapon || firePoint == null) return;

        Vector3 mousePos = GameInput.Instance.GetMousePosition();
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);

    }

    // ОБНОВЛЕНИЕ UI ПАТРОНОВ
    private void UpdateAmmoUI()
    {
        if (ammoText == null) return;

        GameObject background = ammoText.transform.parent.gameObject;
        GameObject backgroundReload = reloadText.transform.parent.gameObject;

        if (hasWeapon && currentWeapon != null)
        {
            background.SetActive(true);

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

    // ОБРАБОТКА ПОЛУЧЕНИЯ УРОНА
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

    // БЛОКИРОВАНИЕ ВО ВРЕМЯ ДИАЛОГА
    public void SetDialogueLock(bool lockState)
    {
        isLockedInDialogue = lockState;

        if (lockState)
        {
            
            if (rb != null) rb.linearVelocity = Vector2.zero;
            isRunning = false;
        }
    }
}
