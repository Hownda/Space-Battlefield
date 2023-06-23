using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.UI;

public class PlayerActions : NetworkBehaviour
{
    private MovementControls gameActions;
    private PlayerNetwork playerNetwork;

    public float pickUpRange = 2;
    public LayerMask interactableObjects;
    public Text keybindText;

    private void Awake()
    {
        gameActions = KeybindManager.inputActions;
        gameActions.Player.Enter.started += Enter;
        gameActions.Player.Pickup.started += PickUp;
        gameActions.Player.Eat.started += Eat;
    }

    private void Start()
    {
        GameObject[] playerRoots = GameObject.FindGameObjectsWithTag("Root");
        foreach (GameObject playerRoot in playerRoots)
        {
            if (playerRoot.GetComponent<NetworkObject>().OwnerClientId == OwnerClientId)
            {
                playerNetwork = playerRoot.GetComponent<PlayerNetwork>();
            }
        }
    }

    private void Update()
    {
        keybindText.text = "Heal: " + KeybindManager.inputActions.Player.Eat.GetBindingDisplayString();
    }

    private void Enter(InputAction.CallbackContext obj)
    {
        if (IsOwner)
        {
            GameObject[] spaceships = GameObject.FindGameObjectsWithTag("Spaceship");
            foreach (GameObject spaceship in spaceships)
            {
                if (spaceship.GetComponent<NetworkObject>().OwnerClientId == OwnerClientId)
                {
                    // Camera components
                    spaceship.GetComponentInChildren<Camera>().enabled = true;
                    spaceship.GetComponentInChildren<SpaceshipCamera>().enabled = true;
                    spaceship.GetComponentInChildren<AudioListener>().enabled = true;

                    // Interaction components
                    spaceship.GetComponentInChildren<SpaceshipMovement>().enabled = true;
                    spaceship.GetComponentInChildren<PlayerInput>().enabled = true;
                    spaceship.GetComponentInChildren<SpaceshipMovement>().spaceshipCanvas.SetActive(true);
                    spaceship.GetComponent<SpaceshipActions>().enabled = true;
                    spaceship.GetComponent<Cannons>().enabled = true;
                    spaceship.GetComponent<Hull>().integrityBillboard.SetActive(false);
                    gameActions.Player.Disable();

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
                    playerNetwork.AddObjectToInventoryServerRpc("Flower", 5, OwnerClientId);
                    //audioManager.Play("pick-up");
                }
            }
        }
    }

    private void Eat(InputAction.CallbackContext obj)
    {
        Healthbar healthbar = GetComponent<Healthbar>();
        if (healthbar.health.Value < 100) 
        {
            if (playerNetwork.flowerCount.Value > 0)
            {
                Game.instance.HealDamageOnPlayerServerRpc(OwnerClientId, 5);
                playerNetwork.RemoveObjectFromInventoryServerRpc("Flower", 1, OwnerClientId);
            }
        }
    }

    [ServerRpc]
    private void DespawnPlayerServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
    }
}
