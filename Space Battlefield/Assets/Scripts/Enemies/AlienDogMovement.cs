using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AlienDogMovement : MonoBehaviour
{
    private enum State
    {
        Aggressive, Passive, Attacking
    }

    private State state;

    public GameObject player;
    public float speed = 20;
    public float slowSpeed = 10;
    public float fastSpeed = 20;
    private Animator animator;

    private Rigidbody rb;
    public GravityOrbit gravityOrbit;

    private Vector3 spawnPoint;
    public float gravity = -5f;
    public float rotationCorrection = 5;

    public bool isGrounded = false;
    public float groundOffset = 0.5f;
    public float jumpStrength = 10;
    public LayerMask groundMask;
    public LayerMask playerMask;

    // Patrol
    private float targetZRotation;
    public float rotationForce = 3;
    private float behaviourChangeCooldown = 6;
    private float currentChangeBehaviourProgress = 1;
    private bool changeBehaviour;
    private float changeBehaviourTime;

    // Attack
    public float alertRange = 15;
    private float attackTime;
    private float attackCooldown = 2f;
    private float attackRange = 5;

    // Sound
    public float normalStepInterval = 1;
    public float fastStepInterval = 0.5f;
    private float stepTime;
    public AudioSource stepSound;
    public AudioSource punchSound;

    private void Start()
    {
        spawnPoint = transform.position;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        changeBehaviourTime = Time.time;
        attackTime = Time.time;
        stepTime = Time.time;
    }

    private void FixedUpdate()
    {
        if (gravityOrbit)
        {
            DetectPlayer();
            HandleMovement();
            HandleSound();

            Vector3 gravityUp = GetGravityUp();

            rb.AddForce(gravityUp * gravity);
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, gravityUp) * transform.rotation;
            transform.rotation = targetRotation;
        }
    }

    private void DetectPlayer()
    {
        if (Physics.CheckSphere(transform.position, alertRange, playerMask)) 
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, alertRange, playerMask);
            player = colliders[0].gameObject;
        }
        else
        {
            player = null;
        }

    }

    private void HandleMovement()
    {
        if (player == null)
        {
            Patrol();
            state = State.Passive;
        }
        else
        {
            Attack();
        }
    }

    private void Patrol()
    {
        if (changeBehaviourTime + behaviourChangeCooldown < Time.time)
        {
            changeBehaviourTime = Time.time;
            currentChangeBehaviourProgress = 1;
            targetZRotation = Random.Range(5, 10);
            changeBehaviour = true;
        }
        if (changeBehaviour == true)
        {
            currentChangeBehaviourProgress -= Time.deltaTime;
            transform.Rotate(targetZRotation * transform.up * rotationForce * Time.deltaTime);
            if (currentChangeBehaviourProgress <= 0)
            {
                changeBehaviour = false;
            }
        }
        Vector3 currentVelocity = rb.velocity;
        Vector3 velocityChange = transform.forward * (20) - currentVelocity;
        rb.AddForce(velocityChange / 2);
        animator.SetBool("Run", false);
        animator.SetBool("Walk", true);
    }
    

    private void Attack()
    {
        Quaternion lookRotation = Quaternion.LookRotation(player.transform.position - transform.position, transform.up);
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, lookRotation.eulerAngles.y, transform.rotation.eulerAngles.z));
        Vector3 currentVelocity = rb.velocity;
        Vector3 velocityChange = transform.forward * speed - currentVelocity;
        rb.AddForce(velocityChange);
        animator.SetBool("Walk", false);
        animator.SetBool("Run", true);

       
        if (Vector3.Distance(transform.position, player.transform.position) <= attackRange)
        {
            state = State.Attacking;
            speed = slowSpeed;
            if (attackTime + attackCooldown < Time.time)
            {
                animator.SetTrigger("Attack");
                attackTime = Time.time;
                StartCoroutine(DamageDelay());
            }
        }
        else
        {
            state = State.Aggressive;
            speed = fastSpeed;
        }
    }

    private IEnumerator DamageDelay()
    {
        yield return new WaitForSeconds(.5f);
        Debug.Log("Attack");
        if (Vector3.Distance(transform.position, player.transform.position) <= attackRange)
        {
            Game.instance.DealDamageToPlayerServerRpc(player.GetComponent<NetworkObject>().OwnerClientId, 15);
            punchSound.Play();
        }
    }

    private void HandleSound()
    {
        if (state == State.Passive)
        {
            if (stepTime + normalStepInterval < Time.time)
            {
                stepTime = Time.time;
                stepSound.Play();
            }
        }
        else if (state == State.Aggressive)
        {
            if (stepTime + fastStepInterval < Time.time)
            {
                stepTime = Time.time;
                stepSound.Play();
            }
        }
        else
        {
            // Don't Play step sound;
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
