using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TeamManager : NetworkBehaviour
{
    public static TeamManager Instance { get; private set; }

    [Header("Spawn Points")]
    public Transform[] redSpawnPoints;   // точки для красной команды (3 штуки)
    public Transform[] blueSpawnPoints;  // точки для синей команды (3 штуки)

    private Queue<Transform> availableRedSpawns;
    private Queue<Transform> availableBlueSpawns;

    private List<ulong> redPlayers = new List<ulong>();
    private List<ulong> bluePlayers = new List<ulong>();

    public int maxTeamSize = 3; // максимум 3 игрока в команде

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            enabled = false; // клиентам не нужно логику менеджера
            return;
        }

        // Создаём очереди из точек спавна
        availableRedSpawns = new Queue<Transform>(redSpawnPoints);
        availableBlueSpawns = new Queue<Transform>(blueSpawnPoints);
    }

    // Вызывается сервером при появлении нового игрока
    public void AssignTeam(NetworkPlayer player)
    {
        if (!IsServer) return;

        Team targetTeam = Team.None;

        // Сначала заполняем красных, потом синих, но соблюдаем лимит
        if (redPlayers.Count < maxTeamSize && (bluePlayers.Count >= maxTeamSize || redPlayers.Count <= bluePlayers.Count))
            targetTeam = Team.Red;
        else if (bluePlayers.Count < maxTeamSize)
            targetTeam = Team.Blue;
        else
        {
            Debug.LogWarning("Все команды полны! Нельзя назначить команду.");
            return;
        }

        // Устанавливаем команду игроку
        player.SetTeam(targetTeam);

        // Добавляем в список
        if (targetTeam == Team.Red)
            redPlayers.Add(player.OwnerClientId);
        else
            bluePlayers.Add(player.OwnerClientId);

        Debug.Log($"Игрок {player.OwnerClientId} в команде {targetTeam}. Red:{redPlayers.Count}, Blue:{bluePlayers.Count}");
    }

    // Получить позицию спавна для команды
    public Vector3 GetSpawnPosition(Team team)
    {
        if (!IsServer) return Vector3.zero;

        Queue<Transform> queue = null;
        if (team == Team.Red) queue = availableRedSpawns;
        else if (team == Team.Blue) queue = availableBlueSpawns;
        else return Vector3.zero;

        if (queue == null || queue.Count == 0)
            return Vector3.zero;

        Transform spawn = queue.Dequeue();
        queue.Enqueue(spawn); // циклически повторяем
                              //return spawn.position;
        Debug.Log($"GetSpawnPosition для {team}: {spawn.position}");
        return spawn.position;
    }

    // Опционально: вызвать при смерти игрока для респавна
    public void RespawnPlayer(NetworkPlayer player)
    {
        if (!IsServer) return;
        Vector3 pos = GetSpawnPosition(player.GetTeam());
        player.transform.position = pos;
    }
}