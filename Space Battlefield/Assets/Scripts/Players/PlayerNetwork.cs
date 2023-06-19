using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

/// <summary>
/// The class <c>Player Network</c> executes functions on player spawn.
/// </summary>
public class PlayerNetwork : NetworkBehaviour
{
    private PlayerDictionary dictionary;

    public GameObject victoryOverlay;
    public GameObject defeatOverlay;
    public Camera endCamera;

    public GameObject inventoryCanvas;
    public NetworkVariable<int> rockCount = new NetworkVariable<int>(0, writePerm: NetworkVariableWritePermission.Server);
    public Text rockCounter;
    public GameObject rockItem;
    public Transform rockCollectionUI;

    void Start()
    {
        if (IsOwner)
        {
            inventoryCanvas.SetActive(true);
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

    private void Update()
    {
        if (IsOwner)
        {
            rockCounter.text = rockCount.Value.ToString();
        }
    }

    [ServerRpc] public void AddObjectToInventoryServerRpc(string item, int amount, ulong clientId)
    {
        if (item == "Rock")
        {
            rockCount.Value += amount;

            AddObjectToInventoryClientRpc(item, clientId);            
        }
    }

    [ClientRpc] private void AddObjectToInventoryClientRpc(string item, ulong clientId)
    {
        if (OwnerClientId == clientId)
        {
            if (item == "Rock")
            {
                GameObject rockAdditionUI = Instantiate(rockItem, rockCollectionUI);
                Destroy(rockAdditionUI, 2f);
            }
        }
    }

    [ServerRpc] public void RemoveObjectFromInventoryServerRpc(string item, int amount)
    {
        if (item == "Rock")
        {
            rockCount.Value -= amount;

            for (int i = 0; i < amount; i++)
            {
                GameObject rockAdditionUI = Instantiate(rockItem, rockCollectionUI);
                rockAdditionUI.GetComponentInChildren<Text>().text = (-1).ToString();
                rockAdditionUI.GetComponentInChildren<Text>().color = new Color(255, 0, 0);
                Destroy(rockAdditionUI, 2f);
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
