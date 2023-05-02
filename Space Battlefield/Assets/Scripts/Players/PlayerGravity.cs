using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerGravity : NetworkBehaviour
{
    private Rigidbody rb;
    public GravityOrbit Gravity;

    private float gravity = -20f;

    private bool jump;
    private float jumpStrength = 10f;
    public float groundOffset = 1.1f;
    public bool isGrounded;
    [SerializeField] LayerMask groundMask;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsOwner)
        {
            jump = true;
        }
    }

    private void FixedUpdate()
    {
        if (Gravity && IsOwner)
        {
            isGrounded = Physics.Raycast(transform.position, -transform.up, groundOffset, groundMask);
            Vector3 gravityUp = (transform.position - Gravity.transform.position).normalized;
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
}
