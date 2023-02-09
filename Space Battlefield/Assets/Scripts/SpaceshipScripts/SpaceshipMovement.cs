using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpaceshipMovement : MonoBehaviour
{
    [SerializeField]
    private float rollTorque = 20000f;
    [SerializeField]
    private float thrust = 50f;
    [SerializeField]
    private float upDownForce = 4000f;
    [SerializeField]
    private float strafeForce = 4000f;
    [SerializeField]
    private float velocityFactor = 10f;

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
        if (thrust1D > 0 && thrustPercent < 100)
        {
            if (thrustPercent + velocityFactor < 100)
            {
                thrustPercent += velocityFactor * Time.deltaTime;
                Debug.Log("increasing" + thrustPercent);
            }
            else
            {
                thrustPercent += 100 - thrustPercent;
            }
        }
        else if (thrust1D < 0 && thrustPercent > 3)
        {
            if (thrustPercent - velocityFactor > 3)
            {
                thrustPercent -= velocityFactor * Time.deltaTime;
            }
            else
            {
                thrustPercent -= thrustPercent - 3;
            }
        }
        Vector3 spaceshipPosition = transform.position;
        spaceshipPosition += thrustPercent * thrust * Time.deltaTime * transform.right;
        transform.position = Vector3.Lerp(transform.position, spaceshipPosition, Time.deltaTime * .2f);

        // UpDown
        Quaternion upDownRotation = transform.rotation;
        upDownRotation *= Quaternion.Euler(0, 0, upDown1D * upDownForce * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, upDownRotation, Time.deltaTime * .2f);

        // Strafe
        Quaternion strafeRotation = transform.rotation;
        strafeRotation *= Quaternion.Euler(0, strafe1D * strafeForce * Time.deltaTime, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, strafeRotation, Time.deltaTime * .2f);
    }
    public void OnThrust(InputAction.CallbackContext context)
    {
        thrust1D = context.ReadValue<float>();
        rb.velocity = Vector3.zero;
    }

    public void OnStrafe(InputAction.CallbackContext context)
    {
        strafe1D = context.ReadValue<float>();
        rb.angularVelocity = Vector3.zero;
    }

    public void OnUpDown(InputAction.CallbackContext context)
    {
        upDown1D = context.ReadValue<float>();
        rb.angularVelocity = Vector3.zero;
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        roll1D = context.ReadValue<float>();
        rb.angularVelocity = Vector3.zero;
    }
}
