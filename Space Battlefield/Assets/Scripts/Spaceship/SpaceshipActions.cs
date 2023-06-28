using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class SpaceshipActions : NetworkBehaviour
{
    private MovementControls gameActions;
    private PlayerNetwork playerNetwork;
    public GameObject playerPrefab;

    public float boostDuration = 5;
    private float boostTime;

    private void OnEnable()
    {
        if (IsOwner)
        {
            gameActions = KeybindManager.inputActions;
            gameActions.Spaceship.Exit.started += ExitInput;
            gameActions.Spaceship.Boost.started += Boost;
            gameActions.Spaceship.Enable();
        }
    }

    private void Start()
    {
        GameObject player = PlayerDictionary.instance.playerDictionary[OwnerClientId];
        playerNetwork = player.GetComponent<PlayerNetwork>();       
    }

    private void Update()
    {
        if (IsOwner)
        {
            if (boostTime + boostDuration <= Time.time)
            {
                GetComponent<SpaceshipMovement>().thrust = 5;
            }
        }
    }

    public void ExitInput(InputAction.CallbackContext obj)
    {
        if (IsOwner)
        {
            Exit();
        }
    }

    public void Exit()
    {
        GetComponent<Hull>().integrityBillboard.SetActive(true);
        GetComponentInChildren<Camera>().enabled = false;
        GetComponentInChildren<SpaceshipMovement>().enabled = false;
        GetComponentInChildren<AudioListener>().enabled = false;
        GetComponent<Cannons>().enabled = false;
        SpawnPlayerServerRpc();
        this.enabled = false;
    }

    
    [ServerRpc] private void SpawnPlayerServerRpc()
    {
        Vector3 spawnPosition;
        if (GetComponent<PlayerGravity>().gravityOrbit != null)
        {
            spawnPosition = transform.position + 3 * ((transform.position - GetComponent<PlayerGravity>().gravityOrbit.transform.position).normalized);
        }
        else
        {
            spawnPosition = transform.position + 3 * transform.up;
        }

        GameObject player = Instantiate(playerPrefab, new Vector3(spawnPosition.x, spawnPosition.y, spawnPosition.z), Quaternion.Euler(Vector3.zero));
        player.GetComponent<NetworkObject>().Spawn();
        player.GetComponent<NetworkObject>().ChangeOwnership(OwnerClientId);

        // Make UI look at player
        GetComponent<Hull>().cam = player.GetComponentInChildren<Camera>();
    }

    private void Boost(InputAction.CallbackContext obj)
    {
        if (IsOwner)
        {
            if (playerNetwork.flowerCount.Value >= 3)
            {
                boostTime = Time.time;
                GetComponent<SpaceshipMovement>().thrust = 12;
                playerNetwork.RemoveObjectFromInventoryServerRpc("Flower", 3, OwnerClientId);
            }
        }
    }

}
