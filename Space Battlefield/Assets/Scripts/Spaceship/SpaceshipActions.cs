using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SpaceshipActions : NetworkBehaviour
{
    private MovementControls gameActions;
    public GameObject playerPrefab;

    private float boostDuration = 5;
    private float boostTime;
    private bool boostActive;
    public Text keybindText;
    public Image thrustIcon;
    public Image thrustSliderFill;


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

    private void Update()
    {
        if (IsOwner)
        {
            keybindText.text = "Boost: " + KeybindManager.inputActions.Spaceship.Boost.GetBindingDisplayString();
            if (boostTime + boostDuration <= Time.time && boostActive)
            {               
                GetComponent<SpaceshipMovement>().thrust = 5;
                thrustSliderFill.color = new Color(1, 0.8f, 0, 1);
                thrustIcon.color = new Color(1, 1, 1, 1);
                boostActive = false;
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
        Debug.Log("Exiting...");
        gameActions.Spaceship.Exit.started -= ExitInput;
        gameActions.Spaceship.Boost.started -= Boost;
        gameActions.Spaceship.Disable();
        GetComponent<Hull>().integrityBillboard.SetActive(true);
        Camera.main.GetComponent<AudioListener>().enabled = false;
        Camera.main.enabled = false;       
        GetComponentInChildren<SpaceshipMovement>().enabled = false;        
        GetComponent<Cannons>().enabled = false;
        SpawnPlayerServerRpc();
        this.enabled = false;
    }

    
    [ServerRpc] private void SpawnPlayerServerRpc()
    {
        Vector3 spawnPosition;
        if (GetComponent<SpaceshipGravity>().gravityOrbit != null)
        {
            spawnPosition = transform.position + 3 * ((transform.position - GetComponent<SpaceshipGravity>().gravityOrbit.transform.position).normalized);
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
        Game.instance.AddPlayerToDict(player);
        Game.instance.SetHealth(player);
        Game.instance.DisableBodyPartsClientRpc();
    }

    private void Boost(InputAction.CallbackContext obj)
    {
        Debug.Log("Boost");
    }

}
