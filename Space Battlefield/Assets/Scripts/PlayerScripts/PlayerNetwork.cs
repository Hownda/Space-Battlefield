using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

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
                dictionary.newPlayerToDictServerRpc(OwnerClientId);
                Debug.Log("Adding Player to " + dictionary);
            }
        }
    }
}
