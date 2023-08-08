using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienDogMovement : MonoBehaviour
{
    public GameObject player;
    public float speed = 20;

    private Rigidbody rb;
    public GravityOrbit gravityOrbit;

    public float gravity = -5f;
    public float rotationCorrection = 5;

    public bool isGrounded = false;
    public float groundOffset = 0.5f;
    public float jumpStrength = 10;
    public LayerMask groundMask;
    public bool jump;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (gravityOrbit)
        {
            HandleMovement();

            Vector3 gravityUp = GetGravityUp();

            rb.AddForce(gravityUp * gravity);
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, gravityUp) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10);
        }
    }

    private void HandleMovement()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        else
        {
            Quaternion lookRotation = Quaternion.LookRotation(player.transform.position - transform.position);
            transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, lookRotation.eulerAngles.y, transform.rotation.eulerAngles.z));
            Vector3 currentVelocity = rb.velocity;
            Vector3 velocityChange = transform.forward * speed - currentVelocity;
            rb.AddForce(velocityChange);

            //Jump with Input
            isGrounded = Physics.Raycast(transform.position, -transform.up, groundOffset, groundMask);
            if (jump)
            {
                if (isGrounded)
                {
                    rb.AddForce(GetGravityUp() * jumpStrength, ForceMode.Impulse);
                }
                jump = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            jump = true;
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
