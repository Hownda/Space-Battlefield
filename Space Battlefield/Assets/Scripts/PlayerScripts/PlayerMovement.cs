using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    public Rigidbody rb;

    public float speed = 20f;
    public float maxForce = 1;

    MovementControls controls;
    MovementControls.GroundMovementActions groundMovement;
    Vector2 horizontalInput;

    private PlayerDictionary playerDictionary;
    private AudioListener otherPlayerAudioListener;

    private void Awake()
    {
        controls = new MovementControls();
        groundMovement = controls.GroundMovement;
        groundMovement.HorizontalMovement.performed += ctx => horizontalInput = ctx.ReadValue<Vector2>();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
    private void FixedUpdate()
    {
        if (IsOwner)
        {
            Vector3 horizontalVelocity = speed * (transform.right * horizontalInput.x + transform.forward * horizontalInput.y);
            Vector3 currentVelocity = rb.velocity;

            Vector3 velocityChange = horizontalVelocity - currentVelocity;

            rb.AddForce(velocityChange);            
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            // Enter and exit spaceship
            if (Input.GetKeyDown("f"))
            {
                Enter();
            }
        }

        if (otherPlayerAudioListener == null)
        {
            return;            
        }
        else
        {
            otherPlayerAudioListener.enabled = false;
        }
    }

    private void Enter()
    {
        GameObject[] spaceships = GameObject.FindGameObjectsWithTag("Spaceship");
        foreach (GameObject spaceship in spaceships)
        {
            if (OwnerClientId == spaceship.GetComponent<NetworkObject>().OwnerClientId)
            {
                DisableGameObjectServerRpc();
                spaceship.GetComponentInChildren<Camera>().enabled = true;
                spaceship.GetComponentInChildren<SpaceshipMovement>().enabled = true;
                spaceship.GetComponentInChildren<PlayerInput>().enabled = true;
                spaceship.GetComponentInChildren<AudioListener>().enabled = true;
            }
            else
            {
                otherPlayerAudioListener = spaceship.GetComponentInChildren<AudioListener>();
            }
        }
    }

    [ServerRpc] private void DisableGameObjectServerRpc()
    {
        DisableGameObjectClientRpc();
    }

    [ClientRpc] private void DisableGameObjectClientRpc()
    {
        gameObject.SetActive(false);
    }    
}
