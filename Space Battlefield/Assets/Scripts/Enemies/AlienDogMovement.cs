using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienDogMovement : MonoBehaviour
{
    public GameObject player;
    public float speed = 20;
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

    // Patrol
    private float targetZRotation;
    public float rotationForce = 3;
    private float behaviourChangeCooldown = 6;
    private float currentChangeBehaviourProgress = 1;
    private bool changeBehaviour;
    private float changeBehaviourTime;

    // Attack
    private float attackTime;
    private float attackCooldown = 1.2f;
    private float attackRange = 5;

    private void Start()
    {
        spawnPoint = transform.position;
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        changeBehaviourTime = Time.time;
        attackTime = Time.time;
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
            Patrol();
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
        Vector3 velocityChange = transform.forward * (speed) - currentVelocity;
        rb.AddForce(velocityChange / 2);
        animator.SetBool("Run", false);
        animator.SetBool("Walk", true);
    }
    

    private void Attack()
    {
        Quaternion lookRotation = Quaternion.LookRotation(player.transform.position - transform.position);
        transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, lookRotation.eulerAngles.y, transform.rotation.eulerAngles.z));
        Vector3 currentVelocity = rb.velocity;
        Vector3 velocityChange = transform.forward * speed - currentVelocity;
        rb.AddForce(velocityChange);
        animator.SetBool("Walk", false);
        animator.SetBool("Run", true);

        if (attackTime + attackCooldown < Time.time)
        {
            if (Vector3.Distance(transform.position, player.transform.position) <= attackRange)
            {
                Debug.Log("Attack");
                animator.SetTrigger("Attack");
                attackTime = Time.time;
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
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
