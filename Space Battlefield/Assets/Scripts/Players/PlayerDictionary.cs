using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Keeps track of player GameObject and network id.
/// </summary>
public class PlayerDictionary : NetworkBehaviour
{
    public Dictionary<ulong, GameObject> playerDictionary = new Dictionary<ulong, GameObject>();
    private int playersInRoom = 2;

    private bool savedToDict = false;

    public static PlayerDictionary instance;

    private void Start()
    {
        instance = this;
    }

    [ServerRpc(RequireOwnership = false)] public void NewPlayerToDictServerRpc()
    {
        playerDictionary.Clear();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Root");
        foreach (GameObject player in players)
        {
            playerDictionary.Add(player.GetComponent<NetworkObject>().OwnerClientId, player);
        }
        if (playerDictionary.Count == playersInRoom && savedToDict == false && IsServer)
        {
            GetComponent<Game>().StartGame();
            savedToDict = true;
        }
    }

    [ServerRpc] public void RemovePlayerFromDictServerRpc(ulong clientId)
    {
        playerDictionary.Remove(clientId);
    }
}
