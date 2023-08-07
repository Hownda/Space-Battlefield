using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SpaceshipGravity : NetworkBehaviour
{
    private Rigidbody rb;
    public GravityOrbit gravityOrbit;

    public float gravity = -5f;
    public float rotationCorrection = 5;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            if (gravityOrbit)
            {
                Vector3 gravityUp = GetGravityUp();

                rb.AddForce(gravityUp * gravity);
                Quaternion targetRotation = Quaternion.FromToRotation(transform.up, gravityUp) * transform.rotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10);
            }
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
