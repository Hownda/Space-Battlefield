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

    public GameObject victoryOverlay;
    public GameObject defeatOverlay;
    public Camera endCamera;

    void Start()
    {
        if (IsOwner)
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

    public void Lose()
    {
        if (IsOwner)
        {
            defeatOverlay.SetActive(true);
            defeatOverlay.GetComponentInChildren<ParticleSystem>().Play();
            endCamera.enabled = true;
            DespawnServerRpc();
        }
    }

    public void Win()
    {
        if (IsOwner)
        {
            victoryOverlay.SetActive(true);
            victoryOverlay.GetComponentInChildren<ParticleSystem>().Play();
            endCamera.enabled = true;
            DespawnServerRpc();
        }
    }

    [ServerRpc] private void DespawnServerRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<NetworkObject>().OwnerClientId == OwnerClientId)
            {
                player.GetComponent<NetworkObject>().Despawn();
                Destroy(player);
            }
        }
    }
}
