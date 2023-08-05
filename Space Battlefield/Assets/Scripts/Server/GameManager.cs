using UnityEngine;
using Unity.Netcode;

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
                connectedPlayers++;
                GameObject playerRoot = Instantiate(playerRootPrefab);
                playerRoot.GetComponent<NetworkObject>().Spawn();
                playerRoot.GetComponent<NetworkObject>().ChangeOwnership(clientId);

                if (connectedPlayers == playerCount)
                {
                    allPlayersConnected = true;
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
    }
}
