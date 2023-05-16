using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerGravity : NetworkBehaviour
{
    private Rigidbody rb;
    public GravityOrbit gravityOrbit;
    private MovementControls gameActions;

    private float gravity = -20f;

    private bool jump;
    private float jumpStrength = 10f;
    public float groundOffset = 1.1f;
    public bool isGrounded;
    [SerializeField] LayerMask groundMask;

    private void Awake()
    {
        gameActions = KeybindManager.inputActions;
        gameActions.Player.Jump.started += Jump;
        gameActions.Player.Enable();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (gravityOrbit && IsOwner)
        {
            isGrounded = Physics.Raycast(transform.position, -transform.up, groundOffset, groundMask);
            Vector3 gravityUp = (transform.position - gravityOrbit.transform.position).normalized;
            Vector3 localUp = transform.up;

            if (jump)
            {
                if (isGrounded)
                {
                    rb.AddForce(gravityUp * jumpStrength, ForceMode.Impulse);                    
                }
                jump = false;
            }

            rb.AddForce(gravityUp * gravity);
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, gravityUp) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 50 * Time.deltaTime);
        }
    }
    private void Jump(InputAction.CallbackContext obj)
    {
        jump = true;
    }
}
