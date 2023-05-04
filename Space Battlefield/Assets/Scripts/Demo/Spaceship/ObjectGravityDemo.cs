using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for spaceship ground and air physics
/// </summary>
public class ObjectGravityDemo : MonoBehaviour
{
    public GravityOrbitDemo gravityOrbit;
    private Rigidbody rb;
    public float gravity = -20f;

    public bool isGrounded;
    public bool colliding;
    public LayerMask ground;
    public float distanceFromGround;

    private float soundStart = 0f;
    private float soundCooldown = 2.5f;

    public ParticleSystem contactParticles;
    public AudioManagerDemo audioManager;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        if (gravityOrbit)
        {
            Vector3 gravityUp = (transform.position - gravityOrbit.transform.position).normalized;

            rb.AddForce(gravityUp * gravity);
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, gravityUp) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10 * Time.deltaTime);
        }
    }

    private void Update()
    {
        isGrounded = Physics.Raycast(transform.position, -transform.up, distanceFromGround, ground);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time > soundStart + soundCooldown)
        {
            if (!isGrounded)
            {
                audioManager.Play("crash-sound");
                soundStart = Time.time;
            }
        }
        Vector3 contactPoint = collision.contacts[0].point;
        ParticleSystem particles = Instantiate(contactParticles, gameObject.transform);
        particles.transform.position = contactPoint;
        particles.Play();
        Destroy(particles.gameObject, 1f);
        colliding = true;
    }

    private void OnCollisionExit()
    {
        colliding = false;
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (isGrounded && GetComponent<SpaceshipMovementDemo>().thrustPercent <= 5)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
