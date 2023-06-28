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

    private void Awake()
    {
        instance = this;
    }

    [ServerRpc(RequireOwnership = false)] public void UpdatePlayerDictionaryServerRpc()
    {
        UpdatePlayerDictionary();

        // Start game when all players connected
        if (IsServer) {
            if (playerDictionary.Count == playersInRoom)
            {
                if (savedToDict == false)
                {
                    GetComponent<Game>().StartGame();
                    savedToDict = true;
                }
            }
        }

        // Update for clients
        UpdatePlayerDictionaryClientRpc();
    }

    [ClientRpc] private void UpdatePlayerDictionaryClientRpc()
    {
        if (!IsServer)
        {
            UpdatePlayerDictionary();
        }
    }

    private void UpdatePlayerDictionary()
    {
        playerDictionary.Clear();

        GameObject[] players = GameObject.FindGameObjectsWithTag("Root");
        foreach (GameObject player in players)
        {
            playerDictionary.Add(player.GetComponent<NetworkObject>().OwnerClientId, player);
        }
    }

    public int GetCount()
    {
        return playerDictionary.Count;
    }

    public GameObject GetOtherPlayer(ulong clientId)
    {
        foreach (KeyValuePair<ulong, GameObject> player in playerDictionary)
        {
            if (player.Key != clientId)
            {
                return player.Value;
            }
        }
        return null;
    }

    [ServerRpc] public void RemovePlayerFromDictServerRpc(ulong clientId)
    {
        playerDictionary.Remove(clientId);
    }
}
