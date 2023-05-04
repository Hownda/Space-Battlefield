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

    [Header("Spawning")]
    public Vector3[] spawnLocations = new[] { new Vector3(-350, -70, -70), new Vector3(350, -70, 70) };
    public Vector3[] spawnRotations = new[] { new Vector3(0, -270, 0), new Vector3(0, -90, 0) };

    private void Awake()
    {
        instance = this;
    }

    [SerializeField] private float spaceshipSpawnOffset;

    /// <summary>
    /// Creates a spaceship for every player
    /// </summary>
    [ServerRpc] public void StartGameServerRpc()
    {
        SetSpawnPositions();
    }

    [ServerRpc(RequireOwnership = false)] public void DealDamageToPlayerServerRpc(ulong clientId, int damage)
    {
        PlayerDictionary.instance.playerDictionary[clientId].GetComponent<Healthbar>().TakeDamage(damage);
    }

    private void SetSpawnPositions()
    {
        int i = 0;
        foreach (KeyValuePair<ulong, GameObject> player in PlayerDictionary.instance.playerDictionary)
        {
            Vector3 moveVector = spawnLocations[i] - player.Value.transform.position;
            player.Value.GetComponent<CapsuleCollider>().enabled = false;
            player.Value.GetComponent<Rigidbody>().position = spawnLocations[i];

            if (player.Value.transform.position != spawnLocations[i])
            {
                player.Value.GetComponent<Rigidbody>().position = spawnLocations[i];

            }

            player.Value.GetComponent<CapsuleCollider>().enabled = true;
            player.Value.transform.rotation = Quaternion.Euler(spawnRotations[i]);
            i++;
        }
        i = 0;
        SpawnSpaceships();
    }

    private void SpawnSpaceships()
    {
        GameObject[] spaceships = GameObject.FindGameObjectsWithTag("Spaceship");
        if (spaceships.Length == 0)
        {
            foreach (KeyValuePair<ulong, GameObject> player in PlayerDictionary.instance.playerDictionary)
            {
                GameObject spaceship = Instantiate(spaceshipPrefab, new Vector3(player.Value.transform.position.x + spaceshipSpawnOffset, player.Value.transform.position.y, player.Value.transform.position.z + spaceshipSpawnOffset), Quaternion.Euler(Vector3.zero));
                spaceship.GetComponent<NetworkObject>().Spawn();
                spaceship.GetComponent<NetworkObject>().ChangeOwnership(player.Key);

            }
        }
    }
}
