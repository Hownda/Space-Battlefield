using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.UI;
using Cinemachine;

public class PlayerActions : NetworkBehaviour
{
    private MovementControls gameActions;

    public float pickUpRange = 2;
    public LayerMask interactableObjects;
    public Text keybindText;

    private void Start()
    {
        if (IsOwner)
        {
            gameActions = KeybindManager.inputActions;
            gameActions.Player.Enter.started += Enter;
            gameActions.Player.Pickup.started += PickUp;
            gameActions.Player.Eat.started += Eat;
            gameActions.Player.Enable();
        }
    }

    private void Update()
    {
        //keybindText.text = "Heal: " + KeybindManager.inputActions.Player.Eat.GetBindingDisplayString();
    }

    private void Enter(InputAction.CallbackContext obj)
    {
        if (IsOwner)
        {
            Debug.Log("Entering");
            foreach (GameObject spaceship in GameObject.FindGameObjectsWithTag("Spaceship"))
            {
                if (spaceship.GetComponent<NetworkObject>().OwnerClientId == OwnerClientId)
                {                    
                    // Interaction components
                    spaceship.GetComponentInChildren<SpaceshipMovement>().enabled = true;
                    spaceship.GetComponentInChildren<PlayerInput>().enabled = true;
                    spaceship.GetComponentInChildren<SpaceshipMovement>().spaceshipCanvas.SetActive(true);
                    spaceship.GetComponent<Cannons>().enabled = true;
                    spaceship.GetComponent<Hull>().integrityBillboard.SetActive(false);
                    spaceship.GetComponent<SpaceshipActions>().enabled = true;

                    CinemachineVirtualCamera camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CinemachineVirtualCamera>();
                    camera.LookAt = spaceship.GetComponent<SpaceshipMovement>().shipLookTarget.transform;
                    camera.Follow = spaceship.GetComponent<SpaceshipMovement>().shipLookTarget.transform;
                    camera.GetComponent<AudioListener>().enabled = true;
                    camera.GetComponent<Camera>().enabled = true;

                    gameActions.Player.Enter.started -= Enter;
                    gameActions.Player.Pickup.started -= PickUp;
                    gameActions.Player.Eat.started -= Eat;
                    gameActions.Player.Disable();

                    Game.instance.SetTempHealthServerRpc(OwnerClientId, GetComponent<Healthbar>().health.Value);
                    DespawnPlayerServerRpc();
                }
            }
        }
    }

    private void PickUp(InputAction.CallbackContext obj)
    {
        if (IsOwner)
        {
            Ray ray = new Ray(GetComponentInChildren<Camera>().transform.position, GetComponentInChildren<Camera>().transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, pickUpRange, interactableObjects))
            {
                //Mining
                if (hit.transform.GetComponentInParent<Flower>())
                {
                    hit.transform.GetComponentInParent<Flower>().PickUp();
                    Game.instance.GiveObjectToPlayerServerRpc(OwnerClientId, Item.Flower, 5);
                    //audioManager.Play("pick-up");
                }
            }
        }
    }

    private void Eat(InputAction.CallbackContext obj)
    {
        Debug.Log("Eat");
    }

    [ServerRpc]
    private void DespawnPlayerServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
    }
}
