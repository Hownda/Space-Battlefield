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
    public GameObject spaceshipPrefab;
    private List<GameObject> spaceships = new List<GameObject>();

    public float enteringDistance = 20f;

    public static Game instance;
    public GameObject playerPrefab;

    [Header("Spawning")]
    private Vector3[] spawnLocations = new[] { new Vector3(350, -60, 70), new Vector3(-350, -60, -70) };
    private Vector3[] spawnRotations = new[] { new Vector3(0, -270, 0), new Vector3(0, -90, 0) };

    private void Awake()
    {
        instance = this;
    }

    [SerializeField] private float spaceshipSpawnOffset;

    /// <summary>
    /// Creates a spaceship for every player
    /// </summary>
    public void StartGame()
    {
        if (IsServer)
        {
            SpawnPlayers();    
        }
    }

    [ServerRpc(RequireOwnership = false)] public void DealDamageToPlayerServerRpc(ulong clientId, int damage)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<NetworkObject>().OwnerClientId == clientId)
            {
                player.GetComponent<Healthbar>().TakeDamage(damage);
                player.GetComponent<Healthbar>().DisplayDamageIndicatorClientRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)] public void DealDamageToSpaceshipServerRpc(ulong clientId, float damage)
    {
        foreach (GameObject spaceship in spaceships)
        {
            if (spaceship.GetComponent<NetworkObject>().OwnerClientId == clientId)
            {
                spaceship.GetComponent<Hull>().TakeDamage(damage);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)] public void HealDamageOnPlayerServerRpc(ulong clientId, int amount)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<NetworkObject>().OwnerClientId == clientId)
            {
                player.GetComponent<Healthbar>().Heal(amount);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)] public void RepairDamageOnSpaceshipServerRpc(ulong clientId, int amount)
    {
        foreach (GameObject spaceship in spaceships)
        {
            if (spaceship.GetComponent<NetworkObject>().OwnerClientId == clientId)
            {
                spaceship.GetComponent<Hull>().Repair(amount);
            }
        }
    }

    private IEnumerator spawnDelay()
    {      
        yield return new WaitForSeconds(2f);
        SpawnSpaceships();        
    }

    private void SpawnPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Root");
        for (int i = 0; i < players.Length; i++)
        {
            GameObject spawnedPlayer = Instantiate(playerPrefab, spawnLocations[i], Quaternion.Euler(spawnRotations[i]));
            spawnedPlayer.GetComponent<NetworkObject>().Spawn();
            spawnedPlayer.GetComponent<NetworkObject>().ChangeOwnership(players[i].GetComponent<NetworkObject>().OwnerClientId);           
        }
        StartCoroutine(spawnDelay());
    }

    private void SpawnSpaceships()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (spaceships.Count == 0)
        {
            for (int i = 0; i < players.Length; i++)   
            {
                ulong clientId = players[i].GetComponent<NetworkObject>().OwnerClientId;

                GameObject spaceship = Instantiate(spaceshipPrefab, new Vector3(players[i].transform.position.x + spaceshipSpawnOffset, players[i].transform.position.y, players[i].transform.position.z + spaceshipSpawnOffset), Quaternion.Euler(Vector3.zero));
                spaceship.GetComponent<NetworkObject>().Spawn();
                spaceship.GetComponent<NetworkObject>().ChangeOwnership(clientId);
                spaceships.Add(spaceship);
            }
        }
        // Disable body parts marked as self
        DisableBodyPartsClientRpc();
    } 
    
    [ServerRpc(RequireOwnership = false)] public void RemoveSpaceshipServerRpc(ulong clientId)
    {
        foreach (GameObject spaceship in spaceships)
        {
            if (spaceship.GetComponent<NetworkObject>().OwnerClientId == clientId)
            {
                spaceship.GetComponent<NetworkObject>().Despawn();
                spaceships.Remove(spaceship);
                
            }
        }
    }
    
    [ClientRpc] private void DisableBodyPartsClientRpc()
    {
        GameObject[]players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            player.GetComponentInChildren<CameraScript>().DisableBodyParts();
        }
    }

    [ServerRpc(RequireOwnership = false)] public void TriggerVictoryServerRpc(ulong loserClientId)
    {
        TriggerVictoryClientRpc(loserClientId);
    }

    [ClientRpc] private void TriggerVictoryClientRpc(ulong loserClientId)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Root");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<NetworkObject>().OwnerClientId == loserClientId)
            {
                player.GetComponent<PlayerNetwork>().Lose();
            }
            else
            {
                player.GetComponent<PlayerNetwork>().Win();
            }
        }
    }
}
