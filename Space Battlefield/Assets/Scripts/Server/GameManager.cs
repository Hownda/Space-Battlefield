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
        try
        {
            currentLobby = await LobbyService.Instance.GetLobbyAsync(PlayerData.instance.currentLobbyId);
            playerCount = currentLobby.Players.Count;


            if (connectedPlayers == playerCount && currentLobby != null)
            {
                Debug.Log("Requesting Start...");
                allPlayersConnected = true;
                GetComponent<Game>().StartGame();
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            StartCoroutine(RequestLobbyService());
        }
    }

    private IEnumerator RequestLobbyService()
    {
        while (currentLobby == null)
        {
            yield return new WaitForSeconds(.5f);
            CheckPlayerCount();
            yield return new WaitForSeconds(.5f);
        }

    }
}
