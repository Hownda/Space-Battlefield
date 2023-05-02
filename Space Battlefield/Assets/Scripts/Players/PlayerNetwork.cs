using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// The class <c>Player Network</c> executes functions on player spawn.
/// </summary>
public class PlayerNetwork : NetworkBehaviour
{   
    private PlayerDictionary dictionary;
    void Start()
    {
        if (IsServer)
        {
            dictionary = GameObject.FindGameObjectWithTag("GameManager").GetComponent<PlayerDictionary>();
            if (dictionary == null)
            {
                Debug.Log("No dictionary found!");
            }
            else
            {
                dictionary.NewPlayerToDictServerRpc();
                Debug.Log("Adding Player to " + dictionary);
            }
        }
    }
}
