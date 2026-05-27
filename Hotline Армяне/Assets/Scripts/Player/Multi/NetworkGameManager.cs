using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class NetworkGameManager : MonoBehaviour
{
    [Header("Credentials (Inspector editable)")]
    public string testUsername = "testplayer";
    public string testPassword = "pass123";

    [Header("UI")]
    public GameObject loginPanel;
    public Text statusText;
    public Button startGameButton;

    private GameNetworkClient networkClient;

    private void Awake()
    {
        networkClient = FindObjectOfType<GameNetworkClient>();
        if (networkClient == null)
        {
            Debug.LogError("GameNetworkClient not found!");
            return;
        }

        // Подписываемся на события
        networkClient.OnLoginSuccess += OnLoginSuccess;
        networkClient.OnLoginError += OnLoginError;
        networkClient.OnLobbyCreated += OnLobbyCreated;
        networkClient.OnConnected += OnConnected;
        networkClient.OnNetworkError += OnNetworkError;

        if (loginPanel != null) loginPanel.SetActive(true);
        if (startGameButton != null) startGameButton.interactable = false;

        // Автоматический вход
        _ = AutoLoginAndCreateLobby();
    }

    private async Task AutoLoginAndCreateLobby()
    {
        statusText.text = "Logging in...";
        bool success = await networkClient.LoginAsync(testUsername, testPassword);
        if (!success)
        {
            statusText.text = "Login failed. Check credentials.";
        }
    }

    private void OnLoginSuccess()
    {
        statusText.text = "Login OK. Creating lobby...";
        _ = networkClient.CreateLobbyAsync();
    }

    private void OnLoginError(string error)
    {
        statusText.text = $"Login error: {error}";
    }

    private void OnLobbyCreated(LobbyCreatedDto lobby)
    {
        statusText.text = $"Lobby created: {lobby.lobby_id}. Connecting...";
        _ = ConnectToWorker(lobby.lobby_id, lobby.worker_url);
    }

    private async Task ConnectToWorker(string lobbyId, string workerUrl)
    {
        if (string.IsNullOrEmpty(workerUrl))
        {
            string url = await networkClient.GetWorkerUrlAsync(lobbyId);
            if (string.IsNullOrEmpty(url))
            {
                statusText.text = "Failed to get worker URL";
                return;
            }
            workerUrl = url;
        }
        await networkClient.ConnectToWorkerAsync(workerUrl, lobbyId);
    }

    private void OnConnected()
    {
        statusText.text = "Connected! Game ready.";
        if (loginPanel != null) loginPanel.SetActive(false);
        if (startGameButton != null) startGameButton.interactable = true;
    }

    private void OnNetworkError(string error)
    {
        statusText.text = $"Network error: {error}";
    }

    public async void OnStartGameButton()
    {
        if (networkClient.IsConnected)
        {
            await networkClient.SendStartGameAsync();
            statusText.text = "Game started!";
        }
    }

    private void OnDestroy()
    {
        if (networkClient != null)
        {
            networkClient.OnLoginSuccess -= OnLoginSuccess;
            networkClient.OnLoginError -= OnLoginError;
            networkClient.OnLobbyCreated -= OnLobbyCreated;
            networkClient.OnConnected -= OnConnected;
            networkClient.OnNetworkError -= OnNetworkError;
        }
    }
}