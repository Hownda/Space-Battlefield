using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AlienDog : NetworkBehaviour
{
    public NetworkVariable<int> health = new(300);

    public enum State
    {
        Aggressive, Passive, Attacking
    }

    public State state;
    public GameObject player;
    public Animator animator;

    private Rigidbody rb;
    public GravityOrbit gravityOrbit;
    public float gravity = -5f;
    public float rotationCorrection = 5;
    public bool isGrounded = false;
    public float groundOffset = 0.5f;
    public float jumpStrength = 10;
    public LayerMask groundMask;
    public LayerMask playerMask;

    public float speed = 20;
    public float slowSpeed = 5;
    public float fastSpeed = 20;

    // Patrol
    private float targetZRotation;
    public float rotationForce = 3;
    private float behaviourChangeCooldown = 6;
    private float currentChangeBehaviourProgress = 1;
    private bool changeBehaviour;
    private float changeBehaviourTime;

    // Attack
    private float alertTime;
    private float alertDuration = 3;
    public float alertRange = 15;

    // Sound
    public float normalStepInterval = 1;
    public float fastStepInterval = 0.5f;
    private float stepTime;
    public AudioSource stepSound;
    public AudioSource punchSound;

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (IsServer)
        {
            rb = GetComponent<Rigidbody>();
            changeBehaviourTime = Time.time;
            stepTime = Time.time;
            alertTime = Time.time;
        }
    }

    private void FixedUpdate()
    {
        if (IsServer) 
        {
            if (gravityOrbit)
            {
                DetectPlayer();
                HandleMovement();
                

                Vector3 gravityUp = GetGravityUp();

                rb.AddForce(gravityUp * gravity);
                Quaternion targetRotation = Quaternion.FromToRotation(transform.up, gravityUp) * transform.rotation;
                transform.rotation = targetRotation;
            }
        }
        HandleSound();
    }

    private void DetectPlayer()
    {
        if (Physics.CheckSphere(transform.position, alertRange, playerMask)) 
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, alertRange, playerMask);
            player = colliders[0].gameObject;
            alertTime = Time.time;
        }
        else
        {
            if (alertTime + alertDuration < Time.time)
            {
                player = null;
            }
        }

    }

    public void SetPlayer(GameObject player)
    {
        this.player = player;
        alertTime = Time.time;
    }

    private void HandleMovement()
    {
        if (player == null)
        {
            Patrol();
            if (state != State.Passive)
            {
                state = State.Passive;
                ChangeStateClientRpc(state);
            }         
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
        PlayAnimationClientRpc("Walk");
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
        PlayAnimationClientRpc("Run");


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

    [ClientRpc] public void ChangeStateClientRpc(State state)
    {
        if (!IsServer)
        {
            this.state = state;
        }
    }

    [ClientRpc] public void PlayAnimationClientRpc(string animation)
    {
        if (animation == "Walk")
        {
            animator.SetBool("Walk", true);
            animator.SetBool("Run", false);
        }
        else if (animation == "Run")
        {
            animator.SetBool("Walk", false);
            animator.SetBool("Run", true);
        }
        else
        {
            animator.SetTrigger("Attack");
        }
    }

    [ClientRpc] public void PlayAttackClientRpc()
    {
        if (!IsServer)
        {
            punchSound.Play();
        }
    }
}
