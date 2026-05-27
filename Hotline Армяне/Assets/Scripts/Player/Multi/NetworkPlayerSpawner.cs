using UnityEngine;
using System.Collections.Generic;

public class NetworkPlayerSpawner : MonoBehaviour
{
    public static NetworkPlayerSpawner Instance { get; private set; }

    [SerializeField] private GameObject remotePlayerPrefab; // Префаб для отображения другого игрока

    private Dictionary<string, GameObject> remotePlayers = new Dictionary<string, GameObject>();
    private GameNetworkClient networkClient;

    private void Awake()
    {
        Instance = this;
        networkClient = FindObjectOfType<GameNetworkClient>();
        if (networkClient != null)
            networkClient.OnStateUpdate += UpdatePlayers;
    }

    public void UpdatePlayers(GameStateDto state)
    {
        if (remotePlayerPrefab == null) return;

        // Удаляем игроков, которых больше нет в состоянии
        List<string> toRemove = new List<string>();
        foreach (var id in remotePlayers.Keys)
        {
            bool found = false;
            foreach (var p in state.players)
                if (p.id == id) { found = true; break; }
            if (!found) toRemove.Add(id);
        }
        foreach (var id in toRemove)
        {
            Destroy(remotePlayers[id]);
            remotePlayers.Remove(id);
        }

        // Обновляем или создаём
        foreach (var playerState in state.players)
        {
            if (networkClient.GetPlayerId() == playerState.id) continue; // пропускаем себя

            if (!remotePlayers.ContainsKey(playerState.id))
            {
                GameObject newPlayer = Instantiate(remotePlayerPrefab, new Vector3(playerState.x, playerState.y, 0), Quaternion.Euler(0, 0, playerState.angle));
                remotePlayers[playerState.id] = newPlayer;
                // Можно добавить компонент для отображения имени и HP
            }
            else
            {
                GameObject go = remotePlayers[playerState.id];
                go.transform.position = new Vector3(playerState.x, playerState.y, 0);
                go.transform.rotation = Quaternion.Euler(0, 0, playerState.angle);
                // Обновить HP, если есть UI
            }
        }
    }

    private void OnDestroy()
    {
        if (networkClient != null)
            networkClient.OnStateUpdate -= UpdatePlayers;
    }
}