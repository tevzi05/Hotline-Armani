using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PlayerController : MonoBehaviour
{
    // ---- Сетевая часть ----
    [Header("Network Settings")]
    [SerializeField] private string testUsername = "testplayer";
    [SerializeField] private string testPassword = "pass123";
    [SerializeField] private GameObject remotePlayerPrefab;
    [SerializeField] private string lobbyId = ""; // пусто -> создать новое лобби (хост)

    private GameNetworkClient networkClient;
    private bool isNetworkGame = false;
    private string myPlayerId;
    private Dictionary<string, GameObject> remotePlayers = new Dictionary<string, GameObject>();
    private float lastSentX, lastSentY, lastSentAngle;
    private float sendCooldown = 0.05f;
    private float lastSendTime;
    private float networkTimeout = 8f;
    private float networkStartTime;
    private bool isHost = false;      // флаг, что этот клиент создал лобби

    // ---- Локальный игрок ----
    public static PlayerController LocalInstance { get; private set; }
    public string PlayerId { get; private set; }

    // ---- Движение ----
    [Header("Movement")]
    [SerializeField] private float movingSpeed = 15f;
    private Rigidbody2D rb;
    private bool isRunning = false;
    private Vector2 moveDirection;

    // ---- Оружие ----
    [Header("Weapon System")]
    [SerializeField] private WeaponData currentWeaponData;
    [SerializeField] private AudioSource weaponAudioSource;
    private bool hasWeapon = false;
    private int currentAmmo;
    private int ammoReserve;
    private int magazineSize;
    private float nextFireTime = 0f;
    public float reloadTime = 1.5f;
    private bool isReloading = false;

    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;

    // ---- Здоровье ----
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    // ---- UI ----
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI reloadText;
    [SerializeField] private TextMeshProUGUI statusText;

    private void Awake()
    {
        LocalInstance = this;
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        if (weaponAudioSource == null)
            weaponAudioSource = GetComponent<AudioSource>();

        networkClient = FindObjectOfType<GameNetworkClient>();
        if (networkClient == null)
        {
            isNetworkGame = false;
            Debug.Log("[PlayerController] Single-player mode");
            if (statusText != null) statusText.text = "Single-player";
            return;
        }
        networkClient.OnPlayerMoved += OnPlayerMoved;
        isNetworkGame = true;
        networkStartTime = Time.time;
        Debug.Log("[PlayerController] Network mode detected");

        // Подписки
        networkClient.OnPlayerIdReceived += OnMyPlayerIdReceived;
        networkClient.OnPlayerJoined += OnPlayerJoined;
        networkClient.OnStateUpdate += OnNetworkStateUpdate;
        networkClient.OnPlayerHit += OnNetworkHit;
        networkClient.OnConnected += OnNetworkConnected;
        networkClient.OnLoginSuccess += OnLoginSuccess;
        networkClient.OnLoginError += OnLoginError;
        networkClient.OnLobbyCreated += OnLobbyCreated;
        networkClient.OnNetworkError += OnNetworkError;
    }

    private void OnPlayerJoined(string username)
    {
        if (username == testUsername) return;
        if (remotePlayerPrefab == null)
        {
            Debug.LogWarning("remotePlayerPrefab is null!");
            return;
        }

        // Спавн на координатах (0,0,0)
        GameObject newPlayer = Instantiate(remotePlayerPrefab, Vector2.zero, Quaternion.identity);
        // Удаляем лишние компоненты управления (если есть)
        var ctrl = newPlayer.GetComponent<PlayerController>();
        if (ctrl != null) Destroy(ctrl);

        // Добавляем простой компонент для синхронизации (если нужен текст имени)
        var sync = newPlayer.GetComponent<RemotePlayerSync>();
        if (sync == null) sync = newPlayer.AddComponent<RemotePlayerSync>();
        sync.Init(username);

        remotePlayers[username] = newPlayer;
        Debug.Log($"[PlayerController] Spawned remote player {username} at (0,0)");
    }

    // Новый метод – обработка move-событий
    private void OnPlayerMoved(string username, float x, float y, float angle)
    {
        if (username == myPlayerId) return; // не перемещаем себя
        if (remotePlayers.TryGetValue(username, out GameObject playerObj))
        {
            playerObj.transform.position = new Vector3(x, y, 0);
            playerObj.transform.rotation = Quaternion.Euler(0, 0, angle);
            Debug.Log($"[PlayerController] Moved {username} to ({x}, {y}, angle {angle})");
        }
        else
        {
            // Если игрок ещё не создан, создаём его (запасной вариант)
            if (remotePlayerPrefab != null)
            {
                GameObject newPlayer = Instantiate(remotePlayerPrefab, new Vector3(x, y, 0), Quaternion.Euler(0, 0, angle));
                var sync = newPlayer.AddComponent<RemotePlayerSync>();
                sync.Init(username);
                remotePlayers[username] = newPlayer;
                Debug.Log($"[PlayerController] Created remote player from move: {username}");
            }
        }
    }

    private async void Start()
    {
        if (!isNetworkGame) return;

        if (statusText != null) statusText.text = "Logging in...";
        bool success = await networkClient.LoginAsync(testUsername, testPassword);
        if (!success)
        {
            if (statusText != null) statusText.text = "Login failed!";
            Debug.LogError("[PlayerController] Login failed, switching to single-player");
            FallbackToSinglePlayer();
            return;
        }

        // Логин успешен – определяем режим
        if (string.IsNullOrEmpty(lobbyId))
        {
            // Хост: создаём новое лобби
            if (statusText != null) statusText.text = "Creating lobby...";
            isHost = true;
            networkClient.OnLobbyCreated += OnLobbyCreatedForHost;
            await networkClient.CreateLobbyAsync();
        }
        else
        {
            // Клиент: подключаемся к существующему лобби
            isHost = false;
            if (statusText != null) statusText.text = $"Joining lobby {lobbyId}...";
            await JoinLobbyAndConnect(lobbyId);
        }
    }

    private void OnLobbyCreatedForHost(LobbyCreatedDto lobby)
    {
        networkClient.OnLobbyCreated -= OnLobbyCreatedForHost;
        lobbyId = lobby.lobby_id;
        if (statusText != null) statusText.text = $"Lobby created: {lobbyId}. Connecting...";
        _ = ConnectToWorker(lobby.lobby_id, lobby.worker_url);
    }

    private async Task JoinLobbyAndConnect(string existingLobbyId)
    {
        string workerUrl = await networkClient.GetWorkerUrlAsync(existingLobbyId);
        if (string.IsNullOrEmpty(workerUrl))
        {
            if (statusText != null) statusText.text = "Failed to get worker URL";
            Debug.LogError("[PlayerController] Failed to get worker URL for lobby " + existingLobbyId);
            FallbackToSinglePlayer();
            return;
        }
        await ConnectToWorker(existingLobbyId, workerUrl);
    }

    private async Task ConnectToWorker(string lobbyId, string workerUrl)
    {
        if (string.IsNullOrEmpty(workerUrl))
        {
            workerUrl = "ws://192.168.1.68:8081";
            Debug.Log($"[PlayerController] Using hardcoded worker URL: {workerUrl}");
        }
        await networkClient.ConnectToWorkerAsync(workerUrl, lobbyId);
    }

    private void OnNetworkConnected()
    {
        if (statusText != null) statusText.text = "Connected to worker!";
        Debug.Log("[PlayerController] Connected to worker");
        // Если хост, запускаем игру
        if (isHost)
        {
            _ = networkClient.SendStartGameAsync();
            if (statusText != null) statusText.text = "Game started (host)";
        }
    }

    private void FallbackToSinglePlayer()
    {
        isNetworkGame = false;
        if (statusText != null) statusText.text = "Offline mode";
        if (networkClient != null)
        {
            networkClient.OnPlayerIdReceived -= OnMyPlayerIdReceived;
            networkClient.OnPlayerJoined -= OnPlayerJoined;
            networkClient.OnStateUpdate -= OnNetworkStateUpdate;
            networkClient.OnPlayerHit -= OnNetworkHit;
            networkClient.OnConnected -= OnNetworkConnected;
            networkClient.OnLoginSuccess -= OnLoginSuccess;
            networkClient.OnLoginError -= OnLoginError;
            networkClient.OnLobbyCreated -= OnLobbyCreated;
            networkClient.OnNetworkError -= OnNetworkError;
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0) return;
        if (LocalInstance != this) return; // только локальный игрок

        if (InputManager.Instance != null)
            moveDirection = InputManager.Instance.GetMovementVector().normalized;

        HandleShooting();
        if (InputManager.Instance != null && InputManager.Instance.IsReloadPressed() && !isReloading && currentAmmo < magazineSize && ammoReserve > 0)
            StartCoroutine(Reload());

        // Таймаут для сетевого режима
        if (isNetworkGame && myPlayerId == null && Time.time - networkStartTime > networkTimeout)
        {
            Debug.LogWarning("[PlayerController] Network timeout, switching to single-player");
            FallbackToSinglePlayer();
        }
    }

    private void FixedUpdate()
    {
        if (InputManager.Instance == null || rb == null) return;
        if (LocalInstance != this) return;

        MoveAndRotate();
    }

    private void MoveAndRotate()
    {
        // Поворот
        Vector3 mousePos = InputManager.Instance.GetMousePosition();
        Vector3 direction = mousePos - transform.position;

        if (direction != Vector3.zero)
        {
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rb.MoveRotation(angle);
        }

        // Движение
        Vector2 moveVelocity = moveDirection * movingSpeed;
        rb.MovePosition(rb.position + moveVelocity * Time.fixedDeltaTime);
        isRunning = moveDirection.magnitude > 0.1f;
        float x = rb.position.x, y = rb.position.y;
        float ang = rb.rotation;

        _ = networkClient.SendMoveAsync(x, y, ang);

    }

    private void HandleShooting()
    {
        if (!hasWeapon || currentWeaponData == null) return;
        bool isFiring = InputManager.Instance != null && InputManager.Instance.IsShootingPressed();

        if (isFiring && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
            {
                Shoot();
                if (isNetworkGame && networkClient != null && networkClient.IsConnected && myPlayerId != null)
                    _ = networkClient.SendShootAsync();
            }
            else
            {
                PlayEmptySound();
                nextFireTime = Time.time + 0.25f;
            }
        }
    }

    private void Shoot()
    {
        Instantiate(currentWeaponData.bulletPrefab, firePoint.position, firePoint.rotation);
        if (currentWeaponData.shootSound != null)
        {
            weaponAudioSource.pitch = Random.Range(currentWeaponData.minPitch, currentWeaponData.maxPitch);
            weaponAudioSource.PlayOneShot(currentWeaponData.shootSound);
        }
        if (isReloading || currentAmmo <= 0) return;
        currentAmmo--;
        nextFireTime = Time.time + currentWeaponData.fireRate;
        UpdateAmmoUI();
    }

    private void PlayEmptySound()
    {
        if (currentWeaponData.emptySound != null && !weaponAudioSource.isPlaying)
            weaponAudioSource.PlayOneShot(currentWeaponData.emptySound);
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
            anim.runtimeAnimatorController = newData.weaponOverride;

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
        ammoReserve += currentWeaponData.ammoPerKill;
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

    private void LateUpdate()
    {
        if (!hasWeapon || firePoint == null) return;
        if (InputManager.Instance == null) return;
        Vector3 mousePos = InputManager.Instance.GetMousePosition();
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
            ammoText.text = $"{currentAmmo}/{ammoReserve}";
            if (ammoReserve == 0 && currentAmmo == 0)
                ammoText.text = "No ammo!";
            backgroundReload.SetActive(currentAmmo == 0);
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
        if (RestartManager.Instance != null)
            RestartManager.Instance.ShowDeathScreen();
        gameObject.SetActive(false);
    }

    // ---- Сетевые коллбэки (создание удалённых игроков и синхронизация) ----
    private void OnMyPlayerIdReceived(string playerId)
    {
        myPlayerId = playerId;
        PlayerId = playerId;
        Debug.Log($"[PlayerController] My player id: {playerId}");
        if (statusText != null) statusText.text = $"Connected as {testUsername}";
    }



    private void OnNetworkStateUpdate(GameStateDto state)
    {
        foreach (var p in state.players)
        {
            // Пропускаем себя
            if (p.id == myPlayerId) continue;

            if (!remotePlayers.ContainsKey(p.id))
            {
                // Спавн нового игрока
                if (remotePlayerPrefab == null)
                {
                    Debug.LogError("remotePlayerPrefab is null!");
                    continue;
                }
                GameObject newPlayer = Instantiate(remotePlayerPrefab, new Vector3(p.x, p.y, 0), Quaternion.Euler(0, 0, p.angle));
                // Удаляем компонент управления (если есть)
                var ctrl = newPlayer.GetComponent<PlayerController>();
                if (ctrl != null) Destroy(ctrl);
                // Добавляем синхронизацию имени
                var sync = newPlayer.GetComponent<RemotePlayerSync>();
                if (sync == null) sync = newPlayer.AddComponent<RemotePlayerSync>();
                sync.Init(p.username);
                remotePlayers[p.id] = newPlayer;
                Debug.Log($"[PlayerController] Spawned player {p.username} (id {p.id}) at ({p.x}, {p.y})");
            }
            else
            {
                // Обновляем позицию и поворот существующего игрока
                GameObject playerObj = remotePlayers[p.id];
                playerObj.transform.position = new Vector3(p.x, p.y, 0);
                playerObj.transform.rotation = Quaternion.Euler(0, 0, p.angle);
                // Обновляем HP, если нужно
                var sync = playerObj.GetComponent<RemotePlayerSync>();
                if (sync != null) sync.UpdateHealth(p.hp);
            }
        }

        // Удаляем игроков, которых нет в state
        List<string> toRemove = new List<string>();
        foreach (var kvp in remotePlayers)
        {
            bool found = false;
            foreach (var p in state.players)
                if (p.id == kvp.Key) { found = true; break; }
            if (!found) toRemove.Add(kvp.Key);
        }
        foreach (string id in toRemove)
        {
            Destroy(remotePlayers[id]);
            remotePlayers.Remove(id);
            Debug.Log($"[PlayerController] Removed player {id}");
        }
    }

    private void OnNetworkHit(string victimId, int remainingHp)
    {
        if (victimId == myPlayerId)
        {
            currentHealth = remainingHp;
            if (currentHealth <= 0) Die();
        }
        else if (remotePlayers.TryGetValue(victimId, out GameObject go))
        {
            var sync = go.GetComponent<RemotePlayerSync>();
            if (sync != null) sync.UpdateHealth(remainingHp);
        }
    }

    private void OnLoginSuccess() { }
    private void OnLoginError(string error) { }
    private void OnLobbyCreated(LobbyCreatedDto lobby) { }
    private void OnNetworkError(string error) { }

    private void OnDestroy()
    {
        if (networkClient != null)
        {
            networkClient.OnPlayerMoved -= OnPlayerMoved;
            networkClient.OnPlayerIdReceived -= OnMyPlayerIdReceived;
            networkClient.OnPlayerJoined -= OnPlayerJoined;
            networkClient.OnStateUpdate -= OnNetworkStateUpdate;
            networkClient.OnPlayerHit -= OnNetworkHit;
            networkClient.OnConnected -= OnNetworkConnected;
            networkClient.OnLoginSuccess -= OnLoginSuccess;
            networkClient.OnLoginError -= OnLoginError;
            networkClient.OnLobbyCreated -= OnLobbyCreated;
            networkClient.OnNetworkError -= OnNetworkError;
        }
    }
}

// Вспомогательный компонент для удалённых игроков
public class RemotePlayerSync : MonoBehaviour
{
    public string PlayerId { get; private set; }
    public string Username { get; private set; }
    private TextMeshPro nameLabel;
    private int currentHp;

    public void Init( string username)
    {
        Username = username;
        GameObject nameObj = new GameObject("NameLabel");
        nameObj.transform.SetParent(transform);
        nameObj.transform.localPosition = new Vector3(0, 1.2f, 0);
        nameLabel = nameObj.AddComponent<TextMeshPro>();
        nameLabel.text = username;
        nameLabel.fontSize = 3;
        nameLabel.alignment = TextAlignmentOptions.Center;
        nameLabel.color = Color.white;
    }

    public void UpdateHealth(int hp)
    {
        currentHp = hp;
    }

    private void Update()
    {
        if (nameLabel != null && Camera.main != null)
            nameLabel.transform.forward = Camera.main.transform.forward;
    }
}