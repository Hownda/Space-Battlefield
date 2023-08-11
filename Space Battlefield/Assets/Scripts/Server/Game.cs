using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// The class <c>Game</c> manages game events on the server.
/// </summary>
public class Game : NetworkBehaviour
{
    public static Game instance;
    private List<PlayerInformation> playerInformationList = new();
    public Dictionary<ulong, PlayerInformation> playerInformationDict = new();

    public GameObject spaceshipPrefab;
    public GameObject playerPrefab;

    public float enteringDistance = 20f;
    [SerializeField] private float spaceshipSpawnOffset;

    public GameObject playerCardPrefab;
    public Transform playerCardHolder;

    [Header("Spawning")]
    public Vector3[] spawnLocations;

    private void Awake()
    {
        instance = this;
    }

    public void StartGame()
    {
        Debug.Log("Starting Game...");
        int i = 0;
        List<Vector3> randomSpawnPositions = spawnLocations.ToList();
        foreach (GameObject playerRoot in GameObject.FindGameObjectsWithTag("Root"))
        {
            int spawnIndex = Random.Range(0, spawnLocations.Length - 1);
            GameObject spawnedPlayer = Instantiate(playerPrefab, randomSpawnPositions[spawnIndex], Quaternion.identity);
            randomSpawnPositions.RemoveAt(spawnIndex);
            spawnedPlayer.GetComponent<NetworkObject>().Spawn();
            spawnedPlayer.GetComponent<NetworkObject>().ChangeOwnership(playerRoot.GetComponent<NetworkObject>().OwnerClientId);
            playerInformationList.Add(new PlayerInformation(playerRoot.GetComponent<NetworkObject>().OwnerClientId, playerRoot, spawnedPlayer, null));
            playerInformationDict.Add(playerRoot.GetComponent<NetworkObject>().OwnerClientId, playerInformationList[i]);
            i++;
        }
        // Wait for players to reach their spawn points before spawning spaceships
        StartCoroutine(spawnDelay());
    }

    private IEnumerator spawnDelay()
    {
        yield return new WaitForSeconds(2f);

        // Disable body parts marked as self
        DisableBodyPartsClientRpc();
        SpawnPlayerCardsClientRpc();
        SpawnSpaceships();
        GetComponent<GameEvents>().StartCountdownClientRpc();
    }

    [ClientRpc] private void SpawnPlayerCardsClientRpc()
    {
        foreach (GameObject playerRoot in GameObject.FindGameObjectsWithTag("Root"))
        {
            GameObject playerCard = Instantiate(playerCardPrefab, playerCardHolder);
            Text[] texts = playerCard.GetComponentsInChildren<Text>();
            texts[0].text = playerRoot.GetComponent<PlayerNetwork>().username.Value.ToString();
            texts[1].text = playerRoot.GetComponent<PlayerNetwork>().score.Value.ToString();
        }
    }

    private void SpawnSpaceships()
    {
        for (int i = 0; i < playerInformationList.Count; i++)
        {
            GameObject spaceship = Instantiate(spaceshipPrefab, new Vector3(playerInformationList[i].player.transform.position.x + spaceshipSpawnOffset, playerInformationList[i].player.transform.position.y, playerInformationList[i].player.transform.position.z + spaceshipSpawnOffset), Quaternion.Euler(Vector3.zero));
            spaceship.GetComponent<NetworkObject>().Spawn();
            spaceship.GetComponent<NetworkObject>().ChangeOwnership(playerInformationList[i].clientId);
            playerInformationList[i].spaceship = spaceship;
        }
    }

    [ClientRpc] public void DisableBodyPartsClientRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            player.GetComponentInChildren<CameraScript>().DisableBodyParts();
        }
    }

    public void AddPlayerToDict(GameObject player)
    {
        playerInformationDict[player.GetComponent<NetworkObject>().OwnerClientId].player = player;
    }

    [ServerRpc(RequireOwnership = false)] public void DealDamageToPlayerServerRpc(ulong clientId, int damage)
    {
        GameObject player = playerInformationDict[clientId].player;
        player.GetComponent<Healthbar>().TakeDamage(damage);
        player.GetComponent<Healthbar>().DisplayDamageIndicatorClientRpc();
    }

    [ServerRpc(RequireOwnership = false)] public void DealDamageToSpaceshipServerRpc(ulong clientId, float damage)
    {
        GameObject spaceship = playerInformationDict[clientId].spaceship;
        spaceship.GetComponent<Hull>().TakeDamage(damage);
    }

    [ServerRpc(RequireOwnership = false)] public void HealDamageOnPlayerServerRpc(ulong clientId, int amount)
    {
        GameObject player = playerInformationDict[clientId].player;
        player.GetComponent<Healthbar>().Heal(amount);
    }

    [ServerRpc(RequireOwnership = false)] public void RepairDamageOnSpaceshipServerRpc(ulong clientId, int amount)
    {
        GameObject spaceship = playerInformationDict[clientId].spaceship;
        spaceship.GetComponent<Hull>().Repair(amount);
    }
    
    [ServerRpc(RequireOwnership = false)] public void RemoveSpaceshipServerRpc(ulong clientId)
    {
        GameObject spaceship = playerInformationDict[clientId].spaceship;
        spaceship.GetComponent<NetworkObject>().Despawn();                
    }

    [ServerRpc(RequireOwnership = false)] public void TriggerVictoryServerRpc(ulong loserClientId)
    {
        TriggerVictoryClientRpc(loserClientId);
    }

    [ClientRpc] private void TriggerVictoryClientRpc(ulong loserClientId)
    {
        // To do: Add losing;
    }

    [ServerRpc(RequireOwnership = false)] public void SetTempHealthServerRpc(ulong clientId, int health)
    {
        if (IsServer)
        {
            playerInformationDict[clientId].root.GetComponent<PlayerNetwork>().tempHealth.Value = health;
        }
    }

    [ServerRpc(RequireOwnership = false)] public void GiveObjectToPlayerServerRpc(ulong clientId, Item item, int amount)
    {
        switch (item)
        {
            case Item.Rock:
                playerInformationDict[clientId].root.GetComponent<PlayerNetwork>().rockCount.Value += amount;
                break;

            case Item.Flower:
                playerInformationDict[clientId].root.GetComponent<PlayerNetwork>().flowerCount.Value += amount;
                break;

            default:
                break;
        }
        playerInformationDict[clientId].root.GetComponent<PlayerNetwork>().AddObjectToInventoryClientRpc(item, amount, clientId);
    }

    [ServerRpc(RequireOwnership = false)] public void RemoveObjectFromInventoryServerRpc(ulong clientId, Item item, int amount)
    {
        switch (item)
        {
            case Item.Rock:
                playerInformationDict[clientId].root.GetComponent<PlayerNetwork>().rockCount.Value -= amount;
                break;

            case Item.Flower:
                playerInformationDict[clientId].root.GetComponent<PlayerNetwork>().flowerCount.Value -= amount;
                break;

            default:
                break;
        }
        playerInformationDict[clientId].root.GetComponent<PlayerNetwork>().RemoveObjectFromInventoryClientRpc(item, amount, clientId);
    }

    public void SetHealth(GameObject player)
    {
        player.GetComponent<Healthbar>().health.Value = playerInformationDict[player.GetComponent<NetworkObject>().OwnerClientId].root.GetComponent<PlayerNetwork>().tempHealth.Value;
    }
}
