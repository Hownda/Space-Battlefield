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

        GameObject player = PlayerDictionary.instance.playerDictionary[OwnerClientId];
        playerNetwork = player.GetComponent<PlayerNetwork>();
        playerNetwork.playerObject = gameObject;
    }

    private void Update()
    {
        keybindText.text = "Heal: " + KeybindManager.inputActions.Player.Eat.GetBindingDisplayString();
    }

    private void Enter(InputAction.CallbackContext obj)
    {
        if (IsOwner)
        {
            GameObject spaceship = playerNetwork.spaceshipObject;
            if (spaceship != null)
            {
                // Camera components
                spaceship.GetComponentInChildren<Camera>().enabled = true;
                spaceship.GetComponentInChildren<AudioListener>().enabled = true;

                // Interaction components
                spaceship.GetComponentInChildren<SpaceshipMovement>().enabled = true;
                spaceship.GetComponentInChildren<PlayerInput>().enabled = true;
                spaceship.GetComponentInChildren<SpaceshipMovement>().spaceshipCanvas.SetActive(true);
                spaceship.GetComponent<Cannons>().enabled = true;
                spaceship.GetComponent<Hull>().integrityBillboard.SetActive(false);
                gameActions.Player.Disable();
                spaceship.GetComponent<SpaceshipActions>().enabled = true;

                playerNetwork.tempHealth.Value = GetComponent<Healthbar>().health.Value;
                DespawnPlayerServerRpc();
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
