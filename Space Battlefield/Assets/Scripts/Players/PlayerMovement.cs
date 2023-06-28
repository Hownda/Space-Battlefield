using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

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
    private bool isGrounded;
    [SerializeField] LayerMask groundMask;
    public float jumpCooldown = 0.3f;
    private float jumpTime;

    private MovementControls gameActions;

    private void Start()
    {
        if (IsOwner)
        {
            gameActions = KeybindManager.inputActions;
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
        if (input.y > 0 || input.y > 0 && input.x != 0 || input.y > 0 && input.x == 0)
        {
            animator.SetInteger("Vertical", 1);
            handAnimator.SetInteger("Vertical", 1);
        }
        else if (input.y < 0 || input.y < 0 && input.x != 0 || input.y > 0 && input.x == 0)
        {
            animator.SetInteger("Vertical", -1);
            handAnimator.SetInteger("Vertical", -1);
        }
        else if (input.y == 0 && input.x == 0)
        {
            animator.SetInteger("Vertical", 0);
            animator.SetInteger("Horizontal", 0);
            handAnimator.SetInteger("Vertical", 0);
            handAnimator.SetInteger("Horizontal", 0);
        }
        else if (input.x > 0 && input.y == 0)
        {
            animator.SetInteger("Horizontal", 1);
            handAnimator.SetInteger("Horizontal", 1);
        }
        else if (input.x < 0 && input.y == 0)
        {
            animator.SetInteger("Horizontal", -1);
            handAnimator.SetInteger("Horizontal", -1);
        }
    }
}
