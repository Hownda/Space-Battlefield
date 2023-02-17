using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerDictionary : NetworkBehaviour
{
    public Dictionary<ulong, GameObject> playerDictionary = new Dictionary<ulong, GameObject>();
    public int dictionaryCount = 0;
    private int playersInRoom = 2;

    public static PlayerDictionary instance;

    private void Start()
    {
        instance = this;
    }

    [ServerRpc] public void newPlayerToDictServerRpc(ulong clientId)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (!playerDictionary.ContainsKey(clientId))
        {
            playerDictionary.Add(clientId, players[clientId]);
            Debug.Log("Added player with client ID: " + clientId + ". New length of dictionary: " + playerDictionary.Count);
        }  
        if (playerDictionary.Count == playersInRoom)
        {
            StartGame();
        }
    }

    [ServerRpc] public void removePlayerFromDictServerRpc(ulong clientId)
    {
        playerDictionary.Remove(clientId);
    }

    private void StartGame()
    {

    }
}
