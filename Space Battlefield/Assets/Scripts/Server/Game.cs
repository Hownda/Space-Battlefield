using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

/// <summary>
/// The class <c>Game</c> manages game events on the server.
/// </summary>
public class Game : NetworkBehaviour
{
    public GameObject spaceshipPrefab;

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
        GameObject[] spaceships = GameObject.FindGameObjectsWithTag("Spaceship");
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (spaceships.Length == 0)
        {
            for (int i = 0; i < players.Length; i++)   
            {
                GameObject spaceship = Instantiate(spaceshipPrefab, new Vector3(players[i].transform.position.x + spaceshipSpawnOffset, players[i].transform.position.y, players[i].transform.position.z + spaceshipSpawnOffset), Quaternion.Euler(Vector3.zero));
                spaceship.GetComponent<NetworkObject>().Spawn();
                spaceship.GetComponent<NetworkObject>().ChangeOwnership(players[i].GetComponent<NetworkObject>().OwnerClientId);

            }
        }
    }
}
