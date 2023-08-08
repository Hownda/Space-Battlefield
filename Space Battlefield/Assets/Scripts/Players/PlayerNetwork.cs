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
    public NetworkVariable<int> score = new(0);
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

    [ClientRpc] public void AddObjectToInventoryClientRpc(Item item, int amount, ulong clientId)
    {
        if (OwnerClientId == clientId)
        {
            switch (item)
            {
                case Item.Rock:
                    for (int i = 0; i < amount; i++)
                    {
                        GameObject rockAdditionUI = Instantiate(rockItem, collectionUI);
                        Destroy(rockAdditionUI, 2f);
                    }
                    break;

                case Item.Flower:
                    for (int i = 0; i < amount; i++)
                    {
                        GameObject flowerAdditionUI = Instantiate(flowerItem, collectionUI);
                        Destroy(flowerAdditionUI, 2f);
                    }
                    break;

                default:
                    break;
            }    
        }
    }

    [ClientRpc]
    public void RemoveObjectFromInventoryClientRpc(Item item, int amount, ulong clientId)
    {
        if (OwnerClientId == clientId)
        {
            switch (item)
            {
                case Item.Rock:
                    for (int i = 0; i < amount; i++)
                    {
                        GameObject rockAdditionUI = Instantiate(rockItem, collectionUI);
                        rockAdditionUI.GetComponentInChildren<Text>().text = (-1).ToString();
                        rockAdditionUI.GetComponentInChildren<Text>().color = new Color(255, 0, 0);
                        Destroy(rockAdditionUI, 2f);
                    }
                    break;

                case Item.Flower:
                    for (int i = 0; i < amount; i++)
                    {
                        GameObject flowerAdditionUI = Instantiate(flowerItem, collectionUI);
                        flowerAdditionUI.GetComponentInChildren<Text>().text = (-1).ToString();
                        flowerAdditionUI.GetComponentInChildren<Text>().color = new Color(255, 0, 0);
                        Destroy(flowerAdditionUI, 2f);
                    }
                    break;

                default:
                    break;
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
