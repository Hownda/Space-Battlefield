using UnityEngine;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections;

public class GameManager : NetworkBehaviour
{
    private bool allPlayersConnected = false;
    private int playersConnected = 0;

    public GameObject spectatorPrefab;
    public GameObject playerRootPrefab;
    public Transform parentCanvas;

    private void Start()
    {
        NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        RequestLobbyService();
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
                    GameObject playerRoot = Instantiate(playerRootPrefab);
                    playerRoot.GetComponent<NetworkObject>().Spawn();
                    playerRoot.GetComponent<NetworkObject>().ChangeOwnership(clientId);
                    playersConnected++;

                    Debug.Log(playersConnected + "/" + PlayerData.instance.currentLobby.Players.Count);
                    if (playersConnected == PlayerData.instance.currentLobby.Players.Count)
                    {
                        allPlayersConnected = true;
                        Debug.Log("Requesting Start...");
                        GetComponent<Game>().StartGame();
                    }
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

    private async void RequestLobbyService()
    {
        try
        {
            PlayerData.instance.currentLobby = await LobbyService.Instance.GetLobbyAsync(PlayerData.instance.currentLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning(e);
            StartCoroutine(GetLobby());
        }
    }

    private IEnumerator GetLobby()
    {
        yield return new WaitForSeconds(0.5f);
        RequestLobbyService();
    }
}
