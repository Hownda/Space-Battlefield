using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CollisionManager : NetworkBehaviour
{
    private Rigidbody rb;

    public ParticleSystem contactParticles;
    public AudioManager audioManager;
    private float soundStart = 0f;
    private float soundCooldown = 2.5f;

    public bool colliding;
    public LayerMask ground;
    public Collider[] colliders;

    private void Start()
    {
        if (IsOwner)
        {
            rb = GetComponent<Rigidbody>();
            foreach (Collider collider in colliders)
            {
                foreach (Collider otherCollider in colliders)
                {
                    Physics.IgnoreCollision(collider, otherCollider);
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsOwner)
        {
            if (!GetComponent<GroundManeuvering>().isGrounded)
            {
                if (Time.time > soundStart + soundCooldown)
                {
                    audioManager.Play("crash-sound");
                    soundStart = Time.time;
                    Vector3 contactPoint = collision.contacts[0].point;
                    ParticleSystem particles = Instantiate(contactParticles, gameObject.transform);
                    particles.transform.position = contactPoint;
                    particles.Play();
                    Destroy(particles.gameObject, 1f);
                    colliding = true;
                }
            }
        }
    }

    private void OnCollisionExit()
    {
        if (IsOwner)
        {
            colliding = false;
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (IsOwner)
        {
            if (GetComponent<GroundManeuvering>().isGrounded && GetComponent<SpaceshipMovement>().thrustPercent <= 5)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}
