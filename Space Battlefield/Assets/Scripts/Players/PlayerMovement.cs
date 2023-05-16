using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

/// <summary>
/// The class <c>Player Movement</c> contains variables and functions that contribute to the movement of the player. While the player is not inside the spaceship, this class will be active.
/// </summary>
public class PlayerMovement : NetworkBehaviour
{
    public Rigidbody rb;
    public Animator animator;
    public Animator handAnimator;

    public float speed = 20f;
    public float maxForce = 1;

    private MovementControls gameActions;

    private void Awake()
    {
        gameActions = KeybindManager.inputActions;
        gameActions.Player.Enter.started += Enter;
        gameActions.Player.Enable();        
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            // Get input
            Vector2 horizontalInput = gameActions.Player.Movement.ReadValue<Vector2>();

            // Animate input
            Animate(horizontalInput);

            // Move with input
            Vector3 horizontalVelocity = speed * (transform.right * horizontalInput.x + transform.forward * horizontalInput.y);
            Vector3 currentVelocity = rb.velocity;
            Vector3 velocityChange = horizontalVelocity - currentVelocity;
            rb.AddForce(velocityChange);
        }
    }

    private void Animate(Vector2 input)
    {
        if (input.y > 0 || input.y > 0 && input.x != 0 || input.y > 0 && input.x == 0)
        {
            animator.SetInteger("Vertical", 1);
            handAnimator.SetInteger("Vertical", 1);
        }
        else if (input.y < 0 || input.y < 0 && input.x != 0 || input.y > 0 && input.x == 0)
        {
            animator.SetInteger("Vertical", -1);
            handAnimator.SetInteger("Vertical", -1);
        }
        else if (input.y == 0 && input.x == 0)
        {
            animator.SetInteger("Vertical", 0);
            animator.SetInteger("Horizontal", 0);
            handAnimator.SetInteger("Vertical", 0);
            handAnimator.SetInteger("Horizontal", 0);
        }
        else if (input.x > 0 && input.y == 0)
        {
            animator.SetInteger("Horizontal", 1);
            handAnimator.SetInteger("Horizontal", 1);
        }
        else if (input.x < 0 && input.y == 0)
        {
            animator.SetInteger("Horizontal", -1);
            handAnimator.SetInteger("Horizontal", -1);
        }
    }

    private void Enter(InputAction.CallbackContext obj)
    {
        if (IsOwner)
        {
            GameObject[] spaceships = GameObject.FindGameObjectsWithTag("Spaceship");
            foreach (GameObject spaceship in spaceships)
            {
                if (OwnerClientId == spaceship.GetComponent<NetworkObject>().OwnerClientId)
                {
                    spaceship.GetComponentInChildren<Camera>().enabled = true;
                    spaceship.GetComponentInChildren<SpaceshipCamera>().enabled = true;
                    spaceship.GetComponentInChildren<SpaceshipMovement>().enabled = true;
                    spaceship.GetComponentInChildren<PlayerInput>().enabled = true;
                    spaceship.GetComponentInChildren<AudioListener>().enabled = true;
                    spaceship.GetComponentInChildren<Cannons>().enabled = true;
                    GetComponentInChildren<CameraScript>().crosshairOverlay.SetActive(false);
                    DespawnPlayerServerRpc();
                }
            }
        }
    }  
    
    [ServerRpc] private void DespawnPlayerServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
    }
}
