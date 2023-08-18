using UnityEngine;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections;

public class GameManager : NetworkBehaviour
{
    private int connectedPlayers = 0;
    private bool allPlayersConnected = false;
    private int playerCount = 0;

    public GameObject spectatorPrefab;
    public GameObject playerRootPrefab;
    public Transform parentCanvas;

    private Lobby currentLobby;

    private void Start()
    {
        NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        playerCount = PlayerData.instance.playerCount;
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        if (IsOwner)
        {
            if (!PlayerData.instance.isDemo)
            {
                if (allPlayersConnected == false)
                {
                    Debug.Log("Spawning in player");
                    connectedPlayers++;
                    GameObject playerRoot = Instantiate(playerRootPrefab);
                    playerRoot.GetComponent<NetworkObject>().Spawn();
                    playerRoot.GetComponent<NetworkObject>().ChangeOwnership(clientId);
                    StartCoroutine(RequestLobbyService());
                }
                else
                {
                    Debug.Log("Client with client id: " + clientId.ToString() + " is now spectating");
                    GameObject spectator = Instantiate(spectatorPrefab);
                    spectator.GetComponent<NetworkObject>().Spawn();
                    spectator.GetComponent<NetworkObject>().ChangeOwnership(clientId);
                }
            }
            else
            {
                GameObject playerRoot = Instantiate(playerRootPrefab);
                playerRoot.GetComponent<NetworkObject>().Spawn();
                playerRoot.GetComponent<NetworkObject>().ChangeOwnership(clientId);
                GetComponent<Game>().StartGame();
            }
        }
    }

    private IEnumerator RequestLobbyService()
    {
        while (currentLobby == null)
        {           
            GetLobby();
            yield return new WaitForSeconds(.5f);
        }
        if (connectedPlayers == playerCount)
        {
            Debug.Log("Requesting Start...");
            allPlayersConnected = true;
            GetComponent<Game>().StartGame();
        }

    }

    private async void GetLobby()
    {
        currentLobby = await LobbyService.Instance.GetLobbyAsync(PlayerData.instance.currentLobbyId);
        playerCount = currentLobby.Players.Count;
    }
}
