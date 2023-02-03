using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpaceshipMovement : MonoBehaviour
{
    [SerializeField]
    private float rollTorque = 6000f;
    [SerializeField]
    private float thrust = 100f;
    [SerializeField]
    private float upDownForce = 4000f;

    Rigidbody rb;

    private float thrust1D;
    private float strafe1D;
    private float upDown1D;
    private float roll1D;
    private float thrustPercent = 0;
    private Vector2 pitchYaw;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        MovementInput();
    }

    private void MovementInput()
    {
        // Roll
        Quaternion rotation = transform.rotation;
        rotation *= Quaternion.Euler(roll1D * rollTorque * Time.deltaTime, 0, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * .8f);

        // Thrust       
        if (thrust1D > 0 && thrustPercent < 1)
        {
            if (thrustPercent + 1f * Time.deltaTime !> 1f)
            {
                thrustPercent += 1f * Time.deltaTime;
            }
            else
            {
                thrustPercent += 1 - thrustPercent;
            }
        }
        else if (thrust1D < 0 && thrustPercent > 0)
        {
            if (thrustPercent - 1f * Time.deltaTime! < 0)
            {
                thrustPercent -= 1f * Time.deltaTime;
            }
            else
            {
                thrustPercent -= thrustPercent;
            }
        }
        rb.AddForce(thrustPercent * thrust * Time.deltaTime * transform.right);

        // UpDown
        Quaternion upDownRotation = transform.rotation;
        upDownRotation *= Quaternion.Euler(0, -upDown1D * upDownForce * Time.deltaTime, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, upDownRotation, Time.deltaTime * .2f);
    }
    public void OnThrust(InputAction.CallbackContext context)
    {
        thrust1D = context.ReadValue<float>();
    }

    public void OnStrafe(InputAction.CallbackContext context)
    {
        strafe1D = context.ReadValue<float>();
    }

    public void OnUpDown(InputAction.CallbackContext context)
    {
        upDown1D = context.ReadValue<float>();
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        roll1D = context.ReadValue<float>();
    }
}
