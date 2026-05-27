// GameNetworkClient.cs (дополненный)
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class GameNetworkClient : MonoBehaviour
{
    [Header("Server Configuration")]
    public string mainServerUrl = "http://192.168.1.68:8080";
    private HttpClient _http;
    private ClientWebSocket _ws;
    private CancellationTokenSource _wsCts;
    private string _jwtToken;
    private string _currentLobbyId;
    private string _playerId;

    private readonly ConcurrentQueue<Action> _mainThreadQueue = new ConcurrentQueue<Action>();
    private readonly ConcurrentQueue<object> _pendingCommands = new ConcurrentQueue<object>();
    private bool _isReadyToSend = false;

    // События
    public Action OnConnected;
    public Action OnDisconnected;
    public Action OnLoginSuccess;
    public Action<string> OnLoginError;
    public Action<LobbyCreatedDto> OnLobbyCreated;
    public Action<string> OnLobbyError;
    public Action<GameStateDto> OnStateUpdate;
    public Action<string, int> OnPlayerHit;              // victimId, remainingHp
    public Action<string, string> OnPlayerKilled;       // attackerId, victimId
    public Action<GameStatsDto> OnGameEnd;
    public Action<string> OnNetworkError;
    public Action<string> OnPlayerIdReceived;
    // НОВОЕ: игрок присоединился к лобби
    public Action<string> OnPlayerJoined;       // playerId, username
    public Action<string, float, float, float> OnPlayerMoved;

    public string GetJwtToken() => _jwtToken;
    public string GetPlayerId() => _playerId;
    public bool IsConnected => _ws != null && _ws.State == WebSocketState.Open;

    private void Awake()
    {
        _http = new HttpClient { BaseAddress = new Uri(mainServerUrl) };
    }

    private void Update()
    {
        while (_mainThreadQueue.TryDequeue(out var action))
            action?.Invoke();
    }

    private void OnDestroy()
    {
        _wsCts?.Cancel();
        _ws?.Dispose();
        _http?.Dispose();
    }

    // API для внешнего вызова (логин, регистрация, лобби)
    public async Task<bool> RegisterAsync(string username, string password)
    {
        var payload = new { username, password };
        var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
        var resp = await _http.PostAsync("/registration", content);
        if (resp.IsSuccessStatusCode) return true;
        var err = await resp.Content.ReadAsStringAsync();
        Dispatch(() => OnNetworkError?.Invoke($"Register: {err}"));
        return false;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var payload = new { username, password };
            var jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            Debug.Log($"[Network] Login: {jsonPayload}");
            var resp = await _http.PostAsync("/login", content);
            var json = await resp.Content.ReadAsStringAsync();
            Debug.Log($"[Network] Login response: {json}");
            if (resp.IsSuccessStatusCode)
            {
                var data = JsonConvert.DeserializeObject<AuthDto>(json);
                if (data?.token != null)
                {
                    _jwtToken = data.token;
                    Dispatch(() => OnLoginSuccess?.Invoke());
                    return true;
                }
            }
            Dispatch(() => OnLoginError?.Invoke($"Error {resp.StatusCode}: {json}"));
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Network] Login error: {ex.Message}");
            Dispatch(() => OnLoginError?.Invoke($"Network error: {ex.Message}"));
            return false;
        }
    }

    public async Task CreateLobbyAsync()
    {
        EnsureAuth();
        using var req = new HttpRequestMessage(HttpMethod.Post, "/create-lobby");
        req.Headers.Add("Authorization", $"Bearer {_jwtToken}");
        var resp = await _http.SendAsync(req);
        var json = await resp.Content.ReadAsStringAsync();
        if (resp.IsSuccessStatusCode)
        {
            var data = JsonConvert.DeserializeObject<LobbyCreatedDto>(json);
            _currentLobbyId = data.lobby_id;
            Dispatch(() => OnLobbyCreated?.Invoke(data));
        }
        else Dispatch(() => OnNetworkError?.Invoke($"Create Lobby: {json}"));
    }

    public async Task<string> GetWorkerUrlAsync(string lobbyId)
    {
        EnsureAuth();
        using var req = new HttpRequestMessage(HttpMethod.Get, $"/connect-lobby/{lobbyId}");
        req.Headers.Add("Authorization", $"Bearer {_jwtToken}");
        var resp = await _http.SendAsync(req);
        var json = await resp.Content.ReadAsStringAsync();
        if (resp.IsSuccessStatusCode)
        {
            var data = JsonConvert.DeserializeObject<WorkerUrlDto>(json);
            return data.worker_url;
        }
        Dispatch(() => OnLobbyError?.Invoke(json));
        return null;
    }

    public async Task ConnectToWorkerAsync(string workerUrl, string lobbyId)
    {
        if (_ws != null && _ws.State == WebSocketState.Open)
        {
            Debug.LogWarning("[Network] Already connected.");
            return;
        }

        workerUrl = "ws://192.168.1.68:8081";
        _currentLobbyId = lobbyId;
        _isReadyToSend = false;
        var wsUri = new Uri($"{workerUrl.TrimEnd('/')}/game/{lobbyId}?token={Uri.EscapeDataString(_jwtToken)}");
        Debug.Log($"[Network] Connecting to {wsUri}");
        _ws = new ClientWebSocket();
        _wsCts = new CancellationTokenSource();
        try
        {
            await _ws.ConnectAsync(wsUri, _wsCts.Token);
            Debug.Log("[Network] WebSocket connected.");
            _ = ReceiveLoopAsync(_wsCts.Token);
            _isReadyToSend = true;
            FlushPendingCommands();
            Dispatch(() => OnConnected?.Invoke());
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Network] Connection failed: {ex.Message}");
            Dispatch(() => OnNetworkError?.Invoke($"WS Connect: {ex.Message}"));
            _isReadyToSend = false;
        }
    }

    public void Disconnect()
    {
        _isReadyToSend = false;
        _wsCts?.Cancel();
        if (_ws?.State == WebSocketState.Open)
            _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnect", CancellationToken.None);
    }

    // Отправка действий на сервер
    public async Task SendMoveAsync(float x, float y, float angle)
    {
        var payload = new
        {
            action = "move",
            x = x.ToString(),
            y = y.ToString(),
            angle = angle.ToString()
        };
        await SendCommandAsync(payload);
    }

    public async Task SendShootAsync() => await SendCommandAsync(new { action = "shoot" });
    public async Task SendMeleeAsync() => await SendCommandAsync(new { action = "melee" });
    public async Task SendStartGameAsync() => await SendCommandAsync(new { action = "start-game" });

    private async Task SendCommandAsync(object cmd)
    {
        if (!_isReadyToSend || _ws?.State != WebSocketState.Open)
        {
            _pendingCommands.Enqueue(cmd);
            Debug.Log($"[Network] Queued command (state={_ws?.State}), queue size={_pendingCommands.Count}");
            return;
        }

        try
        {
            string json = JsonConvert.SerializeObject(cmd);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            await _ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            Debug.Log($"[Network] SENT: {json}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Network] Send error: {ex.Message}");
            _pendingCommands.Enqueue(cmd);
        }
    }

    private void FlushPendingCommands()
    {
        int count = _pendingCommands.Count;
        while (_pendingCommands.TryDequeue(out var cmd))
            _ = SendCommandAsync(cmd);
        Debug.Log($"[Network] Flushed {count} pending commands.");
    }

    // Приём сообщений
    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        var buffer = new byte[8192];
        try
        {
            while (!ct.IsCancellationRequested && _ws?.State == WebSocketState.Open)
            {
                var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);
                if (result.MessageType == WebSocketMessageType.Close) break;
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Debug.Log($"[Network] RECV: {message}");
                HandleServerMessage(message);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { Debug.LogError($"[Network] Receive error: {ex.Message}"); }
        finally
        {
            _isReadyToSend = false;
            Dispatch(() => OnDisconnected?.Invoke());
        }
    }

    private void HandleServerMessage(string json)
    {
        var wrapper = JsonConvert.DeserializeObject<EventWrapper>(json);
        if (wrapper == null) return;
        switch (wrapper.@event)
        {
            case "init":
                var init = JsonConvert.DeserializeObject<InitDto>(json);
                if (init != null && !string.IsNullOrEmpty(init.player_id))
                {
                    _playerId = init.player_id;
                    Dispatch(() => OnPlayerIdReceived?.Invoke(_playerId));
                }
                break;
            case "state":
                var state = JsonConvert.DeserializeObject<GameStateDto>(json);
                Dispatch(() => OnStateUpdate?.Invoke(state));
                break;
            case "joined":
                var joined = JsonConvert.DeserializeObject<JoinedDto>(json);
                if (joined != null)
                    Dispatch(() => OnPlayerJoined?.Invoke(joined.username));
                break;
            case "move":
                var move = JsonConvert.DeserializeObject<MoveDto>(json);
                if (move != null)
                    Dispatch(() => OnPlayerMoved?.Invoke(move.player_id, move.x, move.y, move.angle));
                break;
            case "hit":
                var hit = JsonConvert.DeserializeObject<HitDto>(json);
                Dispatch(() => OnPlayerHit?.Invoke(hit.victim, hit.hp));
                break;
            case "kill":
                var kill = JsonConvert.DeserializeObject<KillDto>(json);
                Dispatch(() => OnPlayerKilled?.Invoke(kill.attacker, kill.victim));
                break;
            case "game_over":
                var end = JsonConvert.DeserializeObject<GameOverDto>(json);
                Dispatch(() => OnGameEnd?.Invoke(end.stats));
                Disconnect();
                break;
        }
    }

    private void EnsureAuth()
    {
        if (string.IsNullOrEmpty(_jwtToken))
            throw new InvalidOperationException("Login first");
    }

    private void Dispatch(Action action) => _mainThreadQueue.Enqueue(action);
}

// DTO (дополнено для joined)
public class AuthDto { [JsonProperty("token")] public string token { get; set; } }
public class LobbyCreatedDto { [JsonProperty("lobby_id")] public string lobby_id { get; set; } [JsonProperty("worker_url")] public string worker_url { get; set; } }
public class WorkerUrlDto { [JsonProperty("worker_url")] public string worker_url { get; set; } }
public class EventWrapper { [JsonProperty("event")] public string @event { get; set; } }
public class HitDto { [JsonProperty("victim")] public string victim { get; set; } [JsonProperty("hp")] public int hp { get; set; } }
public class KillDto { [JsonProperty("attacker")] public string attacker { get; set; } [JsonProperty("victim")] public string victim { get; set; } }
public class GameOverDto { [JsonProperty("stats")] public GameStatsDto stats { get; set; } }
public class GameStateDto { [JsonProperty("players")] public PlayerStateDto[] players { get; set; } }
public class PlayerStateDto
{
    [JsonProperty("id")] public string id { get; set; }
    [JsonProperty("username")] public string username { get; set; }
    [JsonProperty("x")] public float x { get; set; }
    [JsonProperty("y")] public float y { get; set; }
    [JsonProperty("angle")] public float angle { get; set; }
    [JsonProperty("hp")] public int hp { get; set; }
    [JsonProperty("alive")] public bool alive { get; set; }
}
public class GameStatsDto { [JsonProperty("players")] public PlayerStatDto[] players { get; set; } [JsonProperty("winner")] public string winner { get; set; } }
public class PlayerStatDto { [JsonProperty("username")] public string username { get; set; } [JsonProperty("kills")] public int kills { get; set; } [JsonProperty("deaths")] public int deaths { get; set; } }
public class InitDto { [JsonProperty("event")] public string @event { get; set; } [JsonProperty("player_id")] public string player_id { get; set; } }
public class JoinedDto { [JsonProperty("event")] public string @event { get; set; } [JsonProperty("player_id")] public string player_id { get; set; } [JsonProperty("username")] public string username { get; set; } }

public class MoveDto
{
    [JsonProperty("event")] public string @event { get; set; }
    [JsonProperty("player_id")] public string player_id { get; set; }
    [JsonProperty("x")] public float x { get; set; }
    [JsonProperty("y")] public float y { get; set; }
    [JsonProperty("angle")] public float angle { get; set; }
}