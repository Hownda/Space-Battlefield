using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.Netcode.Components;

/// <summary>
/// The class <c>Player Movement</c> contains variables and functions that contribute to the movement of the player. While the player is not inside the spaceship, this class will be active.
/// </summary>
public class PlayerMovement : NetworkBehaviour
{
    private Rigidbody rb;
    private PlayerGravity playerGravity;
    public Animator animator;
    public Animator handAnimator;

    private float speed = 20f;
    public float maxForce = 1;

    // Jump Values
    private bool jump;
    private float jumpStrength = 10f;
    public float groundOffset = 1.1f;
    public bool isGrounded;
    [SerializeField] LayerMask groundMask;
    public float jumpCooldown = 0.3f;
    private float jumpTime;

    private MovementControls gameActions;

    private void Start()
    {
        if (IsOwner)
        {
            gameActions = GetComponent<PlayerActions>().gameActions;
            gameActions.Player.Jump.started += Jump;
        }

        playerGravity = GetComponent<PlayerGravity>();
        rb = GetComponent<Rigidbody>();        
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            rb.maxLinearVelocity = maxForce;
            Vector2 horizontalInput = gameActions.Player.Movement.ReadValue<Vector2>();
            GroundMovement(horizontalInput);           
        }
    }

    private void GroundMovement(Vector2 horizontalInput)
    {
        // Animate input
        Animate(horizontalInput);

        // Move with input
        Vector3 horizontalVelocity = speed * (transform.right * horizontalInput.x + transform.forward * horizontalInput.y);
        Vector3 currentVelocity = rb.velocity;
        Vector3 velocityChange = horizontalVelocity - currentVelocity;
        rb.AddForce(velocityChange);

        //Jump with Input
        isGrounded = Physics.Raycast(transform.position, -transform.up, groundOffset, groundMask);
        if (jump)
        {
            if (isGrounded)
            {
                rb.AddForce(playerGravity.GetGravityUp() * jumpStrength, ForceMode.Impulse);
            }
            jump = false;
        }
    }

    private void Jump(InputAction.CallbackContext obj)
    {
        if (jumpTime + jumpCooldown <= Time.time)
        {
            jumpTime = Time.time;
            jump = true;
        }
    }

    private void Animate(Vector2 input)
    {
        // Player animator
        animator.SetInteger("Horizontal", Mathf.RoundToInt(input.x));
        animator.SetInteger("Vertical", Mathf.RoundToInt(input.y));
        AnimateServerRpc(input);

        // Hand animator
        handAnimator.SetInteger("Horizontal", Mathf.RoundToInt(input.x));
        handAnimator.SetInteger("Vertical", Mathf.RoundToInt(input.y));
    }

    [ServerRpc] private void AnimateServerRpc(Vector2 input)
    {
        AnimateClientRpc(input);
    }

    [ClientRpc] private void AnimateClientRpc(Vector2 input)
    {
        if (!IsOwner)
        {
            animator.SetInteger("Horizontal", Mathf.RoundToInt(input.x));
            animator.SetInteger("Vertical", Mathf.RoundToInt(input.y));
        }
    }
}
