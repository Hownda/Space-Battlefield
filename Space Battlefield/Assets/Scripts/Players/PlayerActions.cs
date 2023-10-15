using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.UI;
using Cinemachine;

public class PlayerActions : NetworkBehaviour
{
    public MovementControls gameActions;
    private ToolSwitching toolbar;

    public float pickUpRange = 2;
    public LayerMask interactableObjects;
    public Text keybindText;

    private void Start()
    {
        if (IsOwner)
        {
            toolbar = GetComponent<ToolSwitching>();

            gameActions = new MovementControls();
            KeybindManager.LoadAllBindings(gameActions);
            KeybindManager.newInputActions = gameActions;
            gameActions.Player.Enter.started += Enter;
            gameActions.Player.Pickup.started += PickUp;
            gameActions.Player.Hotbar1.started += toolbar.SetSlotActive;
            gameActions.Player.Hotbar2.started += toolbar.SetSlotActive;
            gameActions.Player.Enable();
        }
    }

    public override void OnDestroy()
    {
        if (IsOwner)
        {
            gameActions.Player.Enter.started -= Enter;
            gameActions.Player.Pickup.started -= PickUp;
            gameActions.Player.Hotbar1.started -= toolbar.SetSlotActive;
            gameActions.Player.Hotbar2.started -= toolbar.SetSlotActive;
            gameActions.Player.Disable();
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
                    if (Vector3.Distance(transform.position, spaceship.transform.position ) <= 10)
                    {
                        // Interaction components
                        spaceship.GetComponentInChildren<SpaceshipMovement>().enabled = true;
                        spaceship.GetComponentInChildren<SpaceshipMovement>().spaceshipCanvas.SetActive(true);
                        spaceship.GetComponent<Cannons>().enabled = true;
                        spaceship.GetComponent<SpaceshipActions>().enabled = true;
                        spaceship.GetComponent<CompassObject>().enabled = true;

                        CinemachineVirtualCamera camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CinemachineVirtualCamera>();
                        camera.LookAt = spaceship.GetComponent<SpaceshipMovement>().shipLookTarget.transform;
                        camera.Follow = spaceship.GetComponent<SpaceshipMovement>().shipLookTarget.transform;
                        camera.GetComponent<AudioListener>().enabled = true;
                        camera.GetComponent<Camera>().enabled = true;

                        Game.instance.SetTempHealthServerRpc(OwnerClientId, GetComponent<Healthbar>().health.Value);
                        DespawnPlayerServerRpc();
                    }
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

    [ServerRpc]
    private void DespawnPlayerServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
    }
}
