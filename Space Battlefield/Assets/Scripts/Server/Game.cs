using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// The class <c>Game</c> manages game events on the server.
/// </summary>
public class Game : NetworkBehaviour
{
    public static Game instance;

    public GameObject spaceshipPrefab;
    public GameObject playerPrefab;

    public float enteringDistance = 20f;
    [SerializeField] private float spaceshipSpawnOffset;

    [Header("Spawning")]
    private Vector3[] spawnLocations = new[] { new Vector3(350, -60, 70), new Vector3(-350, -60, -70) };
    private Vector3[] spawnRotations = new[] { new Vector3(0, -270, 0), new Vector3(0, -90, 0) };

    private void Awake()
    {
        instance = this;
    }

    public void StartGame()
    {
        if (IsServer)
        {
            SpawnPlayers();    
        }
    }

    private void SpawnPlayers()
    {
        int i = 0;
        foreach (KeyValuePair<ulong, GameObject> player in PlayerDictionary.instance.playerDictionary)
        {
            GameObject spawnedPlayer = Instantiate(playerPrefab, spawnLocations[i], Quaternion.Euler(spawnRotations[i]));
            spawnedPlayer.GetComponent<NetworkObject>().Spawn();
            spawnedPlayer.GetComponent<NetworkObject>().ChangeOwnership(player.Value.GetComponent<NetworkObject>().OwnerClientId);
            player.Value.GetComponent<PlayerNetwork>().playerObject = spawnedPlayer;
            i++;
        }
        // Wait for players to reach their spawn points before spawning spaceships
        StartCoroutine(spawnDelay());
    }

    [ClientRpc]
    public void DisableBodyPartsClientRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            player.GetComponentInChildren<CameraScript>().DisableBodyParts();
        }
    }

    private IEnumerator spawnDelay()
    {
        yield return new WaitForSeconds(2f);

        // Disable body parts marked as self
        DisableBodyPartsClientRpc();

        SpawnSpaceships();
    }

    private void SpawnSpaceships()
    {
        foreach (KeyValuePair<ulong, GameObject> player in PlayerDictionary.instance.playerDictionary)
        {
            GameObject playerObject = player.Value.GetComponent<PlayerNetwork>().playerObject;

            GameObject spaceship = Instantiate(spaceshipPrefab, new Vector3(playerObject.transform.position.x + spaceshipSpawnOffset, playerObject.transform.position.y, playerObject.transform.position.z + spaceshipSpawnOffset), Quaternion.Euler(Vector3.zero));
            spaceship.GetComponent<NetworkObject>().Spawn();
            spaceship.GetComponent<NetworkObject>().ChangeOwnership(player.Key);
            player.Value.GetComponent<PlayerNetwork>().spaceshipObject = spaceship;
        }
    }

    [ServerRpc(RequireOwnership = false)] public void DealDamageToPlayerServerRpc(ulong clientId, int damage)
    {
        GameObject player = PlayerDictionary.instance.playerDictionary[clientId].GetComponent<PlayerNetwork>().playerObject;
        player.GetComponent<Healthbar>().TakeDamage(damage);
        player.GetComponent<Healthbar>().DisplayDamageIndicatorClientRpc();
    }

    [ServerRpc(RequireOwnership = false)] public void DealDamageToSpaceshipServerRpc(ulong clientId, float damage)
    {
        GameObject spaceship = PlayerDictionary.instance.playerDictionary[clientId].GetComponent<PlayerNetwork>().spaceshipObject;
        spaceship.GetComponent<Hull>().TakeDamage(damage);
    }

    [ServerRpc(RequireOwnership = false)] public void HealDamageOnPlayerServerRpc(ulong clientId, int amount)
    {
        GameObject player = PlayerDictionary.instance.playerDictionary[clientId].GetComponent<PlayerNetwork>().playerObject;
        player.GetComponent<Healthbar>().Heal(amount);
    }

    [ServerRpc(RequireOwnership = false)] public void RepairDamageOnSpaceshipServerRpc(ulong clientId, int amount)
    {
        GameObject spaceship = PlayerDictionary.instance.playerDictionary[clientId].GetComponent<PlayerNetwork>().spaceshipObject;
        spaceship.GetComponent<Hull>().Repair(amount);
    }
    
    [ServerRpc(RequireOwnership = false)] public void RemoveSpaceshipServerRpc(ulong clientId)
    {
        GameObject spaceship = PlayerDictionary.instance.playerDictionary[clientId].GetComponent<PlayerNetwork>().spaceshipObject;
        spaceship.GetComponent<NetworkObject>().Despawn();                
    }

    [ServerRpc(RequireOwnership = false)] public void TriggerVictoryServerRpc(ulong loserClientId)
    {
        TriggerVictoryClientRpc(loserClientId);
    }

    [ClientRpc] private void TriggerVictoryClientRpc(ulong loserClientId)
    {
        foreach (KeyValuePair<ulong, GameObject> player in PlayerDictionary.instance.playerDictionary)
        {
            if (player.Key == loserClientId)
            {
                player.Value.GetComponent<PlayerNetwork>().Lose();
            }
            else
            {
                player.Value.GetComponent<PlayerNetwork>().Win();
            }
        }
    }
}
