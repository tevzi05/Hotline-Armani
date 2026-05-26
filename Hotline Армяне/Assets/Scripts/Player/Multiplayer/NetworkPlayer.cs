using System.Collections;
using TMPro;
using Unity.Netcode; // Сетевой движок
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayer : NetworkBehaviour
{
    // Глобальная ссылка только на НАШЕГО локального сетевого игрока
    public static NetworkPlayer LocalInstance { get; private set; }

    [Header("Movement")]
    [SerializeField] private float movingSpeed = 15f;
    private Rigidbody2D rb;
    private bool isRunning = false;
    private Vector2 moveDirection;

    [Header("Weapon System")]
    [SerializeField] private WeaponData currentWeaponData;
    [SerializeField] private AudioSource weaponAudioSource;
    private bool hasWeapon = false;
    private int currentAmmo;
    private int ammoReserve;
    private int magazineSize;
    private float nextFireTime = 0f;

    [Header("Reloading")]
    public float reloadTime = 1.5f;
    private bool isReloading = false;

    [Header("Setup")]
    [SerializeField] private Transform firePoint;

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    // Сетевое здоровье: сервер меняет, все клиенты видят
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("UI (Укажите ссылки прямо в префабе)")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI reloadText;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        rb = GetComponent<Rigidbody2D>();

        if (weaponAudioSource == null)
            weaponAudioSource = GetComponent<AudioSource>();

        // ЕСЛИ ЭТО Я
        if (IsOwner)
        {
            LocalInstance = this;
            if (IsServer) currentHealth.Value = maxHealth;

            // Включаем интерфейс патронов только у себя
            if (ammoText != null && ammoText.transform.parent != null)
            {
                ammoText.transform.parent.gameObject.SetActive(true);
            }
            UpdateAmmoUI();
        }
        // ЕСЛИ ЭТО ЧУЖОЙ СЕТЕВОЙ ИГРОК
        else
        {
            // Выключаем его камеру, чтобы не перехватывала экран
            Camera otherCam = GetComponentInChildren<Camera>();
            if (otherCam != null)
            {
                otherCam.enabled = false;
                AudioListener listener = otherCam.GetComponent<AudioListener>();
                if (listener != null) listener.enabled = false;
            }

            // Выключаем его Canvas, чтобы его патроны не накладывались на наши
            Canvas otherCanvas = GetComponentInChildren<Canvas>();
            if (otherCanvas != null) otherCanvas.gameObject.SetActive(false);
        }

        currentHealth.OnValueChanged += OnHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        currentHealth.OnValueChanged -= OnHealthChanged;
    }

    private void Update()
    {
        if (!IsOwner) return; // Управляем только своим персонажем
        if (Time.timeScale == 0) return;

        if (GameInput.Instance != null)
            moveDirection = GameInput.Instance.GetMovementVector().normalized;

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
                currentAmmo--;
                nextFireTime = Time.time + currentWeaponData.fireRate;
                UpdateAmmoUI();

                // Просим сервер заспавнить пулю у всех
                ShootServerRpc(firePoint.position, firePoint.rotation);
            }
            else
            {
                PlayEmptySound();
                nextFireTime = Time.time + 0.25f;
            }
        }
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 position, Quaternion rotation)
    {
        GameObject bullet = Instantiate(currentWeaponData.bulletPrefab, position, rotation);
        // Пуля должна иметь компонент NetworkObject!
        if (bullet.GetComponent<NetworkObject>() != null)
        {
            bullet.GetComponent<NetworkObject>().Spawn();
        }
        PlayShootSoundClientRpc();
    }

    [ClientRpc]
    private void PlayShootSoundClientRpc()
    {
        if (currentWeaponData != null && currentWeaponData.shootSound != null)
        {
            weaponAudioSource.pitch = Random.Range(currentWeaponData.minPitch, currentWeaponData.maxPitch);
            weaponAudioSource.PlayOneShot(currentWeaponData.shootSound);
        }
    }

    private void PlayEmptySound()
    {
        if (currentWeaponData.emptySound != null && !weaponAudioSource.isPlaying)
        {
            weaponAudioSource.PlayOneShot(currentWeaponData.emptySound);
        }
    }

    public void EquipWeapon(WeaponData newData)
    {
        currentWeaponData = newData;
        hasWeapon = true;
        currentAmmo = newData.maxAmmo;
        magazineSize = newData.maxAmmo;
        ammoReserve = newData.maxAmmo;

        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null && newData.weaponOverride != null)
        {
            // Внимание: смена контроллеров в сети может ломать NetworkAnimator. 
            // К анимациям оружия мы еще вернемся, пока оставляем старую логику:
            anim.runtimeAnimatorController = newData.weaponOverride;
        }

        if (weaponAudioSource != null && newData.weaponPickup != null)
        {
            weaponAudioSource.pitch = 1f;
            weaponAudioSource.PlayOneShot(newData.weaponPickup);
        }
        UpdateAmmoUI();
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);

        int amountNeeded = magazineSize - currentAmmo;
        int amountToTake = Mathf.Min(amountNeeded, ammoReserve);

        currentAmmo += amountToTake;
        ammoReserve -= amountToTake;

        UpdateAmmoUI();
        isReloading = false;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        if (GameInput.Instance == null || rb == null) return;

        Vector3 mousePos = GameInput.Instance.GetMousePosition();
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rb.MoveRotation(angle);

        Vector2 moveVelocity = moveDirection * movingSpeed;
        rb.MovePosition(rb.position + moveVelocity * Time.fixedDeltaTime);

        isRunning = moveDirection.magnitude > 0.1f;
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;
        if (!hasWeapon || firePoint == null) return;

        Vector3 mousePos = GameInput.Instance.GetMousePosition();
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void UpdateAmmoUI()
    {
        if (ammoText == null || !IsOwner) return;

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
        if (!IsServer) return;
        currentHealth.Value -= damage;
    }

    private void OnHealthChanged(int oldHealth, int newHealth)
    {
        if (newHealth <= 0) Die();
    }

    private void Die()
    {
        if (IsOwner && RestartManager.Instance != null)
        {
            RestartManager.Instance.ShowDeathScreen();
        }

        if (IsServer) GetComponent<NetworkObject>().Despawn();
        else gameObject.SetActive(false);
    }
}
