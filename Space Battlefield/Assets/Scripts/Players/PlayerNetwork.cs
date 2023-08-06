using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Collections;
using System;

/// <summary>
/// The class <c>Player Network</c> executes functions on player spawn.
/// </summary>
public class PlayerNetwork : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> username = new NetworkVariable<FixedString64Bytes>(writePerm: NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> spectator = new(false);

    public GameObject inventoryCanvas;
    public Transform collectionUI;

    public NetworkVariable<int> rockCount = new NetworkVariable<int>(0, writePerm: NetworkVariableWritePermission.Server);
    public Text rockCounter;
    public GameObject rockItem;

    public NetworkVariable<int> flowerCount = new NetworkVariable<int>(0, writePerm: NetworkVariableWritePermission.Server);
    public Text flowerCounter;
    public GameObject flowerItem;

    public GameObject victoryOverlay;
    public GameObject defeatOverlay;
    public Camera endCamera;

    public NetworkVariable<int> tempHealth = new NetworkVariable<int>(writePerm: NetworkVariableWritePermission.Owner);

    void Start()
    {
        if (IsOwner)
        {
            inventoryCanvas.SetActive(true);
            username.Value = PlayerData.instance.username;
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            rockCounter.text = rockCount.Value.ToString();
            flowerCounter.text = flowerCount.Value.ToString();
        }
    }

    [ServerRpc] public void AddObjectToInventoryServerRpc(string item, int amount, ulong clientId)
    {
        if (item == "Rock")
        {
            rockCount.Value += amount;                      
        }

        if (item == "Flower")
        {
            flowerCount.Value += amount;            
        }

        AddObjectToInventoryClientRpc(item, amount, clientId);
    }

    [ClientRpc] private void AddObjectToInventoryClientRpc(string item, int amount, ulong clientId)
    {
        if (OwnerClientId == clientId)
        {
            if (item == "Rock")
            {
                for (int i = 0; i < amount; i++)
                {
                    GameObject rockAdditionUI = Instantiate(rockItem, collectionUI);
                    Destroy(rockAdditionUI, 2f);
                }
            }

            if (item == "Flower")
            {
                for (int i = 0; i < amount; i++)
                {
                    GameObject flowerAdditionUI = Instantiate(flowerItem, collectionUI);
                    Destroy(flowerAdditionUI, 2f);
                }                
            }    
        }
    }

    [ServerRpc] public void RemoveObjectFromInventoryServerRpc(string item, int amount, ulong clientId)
    {
        if (item == "Rock")
        {
            rockCount.Value -= amount;            
        }
        if (item == "Flower")
        {
            flowerCount.Value -= amount;
        }

        RemoveObjectFromInventoryClientRpc(item, amount, clientId);
    }

    [ClientRpc]
    private void RemoveObjectFromInventoryClientRpc(string item, int amount, ulong clientId)
    {
        if (OwnerClientId == clientId)
        {
            if (item == "Rock")
            {
                for (int i = 0; i < amount; i++)
                {
                    GameObject rockAdditionUI = Instantiate(rockItem, collectionUI);
                    rockAdditionUI.GetComponentInChildren<Text>().text = (-1).ToString();
                    rockAdditionUI.GetComponentInChildren<Text>().color = new Color(255, 0, 0);
                    Destroy(rockAdditionUI, 2f);
                }
            }
            if (item == "Flower")
            {
                for (int i = 0; i < amount; i++)
                {
                    GameObject flowerAdditionUI = Instantiate(flowerItem, collectionUI);
                    flowerAdditionUI.GetComponentInChildren<Text>().text = (-1).ToString();
                    flowerAdditionUI.GetComponentInChildren<Text>().color = new Color(255, 0, 0);
                    Destroy(flowerAdditionUI, 2f);
                }
            }
        }
    }

    public void Lose()
    {
        if (IsOwner)
        {
            inventoryCanvas.SetActive(false);
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
            inventoryCanvas.SetActive(false);
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
