using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class Game : NetworkBehaviour
{
    public GameObject spaceshipPrefab;

    public float enteringDistance = 20f;

    public static Game instance;

    private void Awake()
    {
        instance = this;
    }

    [SerializeField] private float spaceshipSpawnOffset;

    [ServerRpc] public void StartGameServerRpc()
    {
        foreach (KeyValuePair<ulong, GameObject> player in PlayerDictionary.instance.playerDictionary)
        {            
                GameObject spaceship = Instantiate(spaceshipPrefab, new Vector3(player.Value.transform.position.x + spaceshipSpawnOffset, player.Value.transform.position.y, player.Value.transform.position.z + spaceshipSpawnOffset), Quaternion.Euler(Vector3.zero));
                spaceship.GetComponent<NetworkObject>().Spawn();
                spaceship.GetComponent<NetworkObject>().ChangeOwnership(player.Key);
            
        }
    }
}
