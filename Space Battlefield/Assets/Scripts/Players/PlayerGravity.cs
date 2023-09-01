using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerGravity : NetworkBehaviour
{
    private Rigidbody rb;
    public GravityOrbit gravityOrbit;
    private PlayerMovement playerMovement;

    public float gravity = -20f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();

    }

    private void FixedUpdate()
    {
        if (gravityOrbit && IsOwner)
        {
            Vector3 gravityUp = GetGravityUp();

            if (!playerMovement.isGrounded)
            {
                rb.AddForce(gravityUp * gravity);
            }
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, gravityUp) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10);
        }
    }

    public Vector3 GetGravityUp()
    {
        return (transform.position - gravityOrbit.transform.position).normalized;
    }

    public GravityOrbit GetGravityOrbit()
    {
        return gravityOrbit;
    }
}
