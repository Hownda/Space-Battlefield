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
    private float swimSpeed = 5f;
    public float maxForce = 1;

    // Jump Values
    private bool jump;
    private int swimVertical;
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

            gameActions.Player.Jump.started += _ => swimVertical = 1;
            gameActions.Player.Jump.canceled += _ => swimVertical = 0;
            gameActions.Player.SwimDown.started += _ => swimVertical = -1;
            gameActions.Player.SwimDown.canceled += _ => swimVertical = 0;
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

            if (playerGravity.gravityOrbit == null)
            {
                SpaceMovement(horizontalInput); 
            }
            else
            {
                GroundMovement(horizontalInput);
            }
            
        }
    }

    private void SpaceMovement(Vector2 horizontalInput)
    {
        Vector3 horizontalVelocity = swimSpeed * (transform.right * horizontalInput.x + transform.forward * horizontalInput.y);
        Vector3 verticalVelocity = swimSpeed * transform.up * swimVertical;
        Vector3 currentVelocity = rb.velocity;
        Vector3 velocityChange = horizontalVelocity + verticalVelocity - currentVelocity;
        rb.AddForce(velocityChange);

        animator.SetBool("Swim", true);
        handAnimator.SetBool("Swim", true);
        AnimateServerRpc(horizontalInput, true);
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
        animator.SetBool("Swim", false);
        handAnimator.SetBool("Swim", false);

        // Player animator
        animator.SetInteger("Horizontal", Mathf.RoundToInt(input.x));
        animator.SetInteger("Vertical", Mathf.RoundToInt(input.y));
        AnimateServerRpc(input, false);

        // Hand animator
        handAnimator.SetInteger("Horizontal", Mathf.RoundToInt(input.x));
        handAnimator.SetInteger("Vertical", Mathf.RoundToInt(input.y));
    }

    [ServerRpc] private void AnimateServerRpc(Vector2 input, bool swim)
    {
        AnimateClientRpc(input, swim);
    }

    [ClientRpc] private void AnimateClientRpc(Vector2 input, bool swim)
    {
        if (!IsOwner)
        {
            if (swim)
            {
                animator.SetBool("Swim", true);
            }
            else
            {
                animator.SetBool("Swim", false);
                animator.SetInteger("Horizontal", Mathf.RoundToInt(input.x));
                animator.SetInteger("Vertical", Mathf.RoundToInt(input.y));
            }
        }
    }
}
