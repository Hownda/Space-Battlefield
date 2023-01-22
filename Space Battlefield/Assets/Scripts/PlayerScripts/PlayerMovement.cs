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
}
