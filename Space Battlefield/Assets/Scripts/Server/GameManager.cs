using UnityEngine;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class GameManager : NetworkBehaviour
{
    private int connectedPlayers = 0;
    private bool allPlayersConnected = false;
    private int playerCount = 0;

    public GameObject spectatorPrefab;
    public GameObject playerRootPrefab;
    public Transform parentCanvas;

    private void Start()
    {
        NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        playerCount = PlayerData.instance.playerCount;
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        if (IsOwner)
        {           
            if (allPlayersConnected == false)
            {
                Debug.Log("Spawning in player");
                connectedPlayers++;
                GameObject playerRoot = Instantiate(playerRootPrefab);
                playerRoot.GetComponent<NetworkObject>().Spawn();
                playerRoot.GetComponent<NetworkObject>().ChangeOwnership(clientId);
                CheckPlayerCount();
            }
            else
            {
                Debug.Log("Client with client id: " + clientId.ToString() + " is now spectating");
                GameObject spectator = Instantiate(spectatorPrefab);
                spectator.GetComponent<NetworkObject>().Spawn();
                spectator.GetComponent<NetworkObject>().ChangeOwnership(clientId);
            }
        }
    }

    private async void CheckPlayerCount()
    {
        Lobby lobby = await LobbyService.Instance.GetLobbyAsync(PlayerData.instance.currentLobbyId);
        playerCount = lobby.Players.Count;

        if (connectedPlayers == playerCount)
        {
            Debug.Log("Requesting Start...");
            allPlayersConnected = true;
            GetComponent<Game>().StartGame();
        }
    }
}
