using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The class <c>Player Movement</c> contains variables and functions that contribute to the movement of the player. While the player is not inside the spaceship, this class will be active.
/// </summary>
public class PlayerMovementDemo : MonoBehaviour
{
    public Rigidbody rb;

    public float speed = 20f;
    public float maxForce = 1;

    MovementControls controls;
    MovementControls.GroundMovementActions groundMovement;
    Vector2 horizontalInput;

    private AudioListener otherPlayerAudioListener;

    private void Awake()
    {
        controls = new MovementControls();
        groundMovement = controls.GroundMovement;
        groundMovement.Movement.performed += ctx => horizontalInput = ctx.ReadValue<Vector2>();
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
        Vector3 horizontalVelocity = speed * (transform.right * horizontalInput.x + transform.forward * horizontalInput.y);
        Vector3 currentVelocity = rb.velocity;

        Vector3 velocityChange = horizontalVelocity - currentVelocity;

        rb.AddForce(velocityChange);
    }

    private void Update()
    {
        // Enter and exit spaceship
        if (Input.GetKeyDown("f"))
        {
            Enter();
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
        GameObject spaceship = GameObject.FindGameObjectWithTag("Spaceship");
        spaceship.GetComponentInChildren<Camera>().enabled = true;
        spaceship.GetComponentInChildren<SpaceshipMovementDemo>().enabled = true;
        spaceship.GetComponentInChildren<PlayerInput>().enabled = true;
        spaceship.GetComponentInChildren<AudioListener>().enabled = true;
        Destroy(gameObject);
    }
}
