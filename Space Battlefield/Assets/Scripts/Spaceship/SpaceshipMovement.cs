using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.UI;

public class SpaceshipMovement : NetworkBehaviour
{
    // Movement Factors
    public float thrust = 5;
    public float upDownInput;
    private float speed = 0;

    private float rollTorque = 10000;   
    private float upDownForce = 6000;   
    public float strafeForce = 10000;
    public float velocityFactor = 15;
    private float maxVelocity = 0;
    private float maxAngularVelocity = 8;

    Rigidbody rb;

    public AudioManager audioManager;
    private MovementControls gameActions;
    public Slider thrustSlider;
    public GameObject spaceshipCanvas;

    public float thrustPercent = 0;
    private float flySoundStart = 0f;

    public GameObject shipLookTarget;

    private void OnEnable()
    {
        gameActions = KeybindManager.inputActions;
        gameActions.Spaceship.Enable();       
    }

    private void OnDisable()
    {
        thrustPercent = 0;
        thrustSlider.value = 0;
        spaceshipCanvas.SetActive(false);
        GetComponentInChildren<PlayerInput>().enabled = false;
        gameActions.Spaceship.Disable();

        // Stop playing thrust sound effect
        audioManager.Stop();
        flySoundStart = 0;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        flySoundStart = -(audioManager.GetAudioLength("spaceship-sound"));
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            Roll();
            Thrust();
            UpDown();
            Strafe();

            FlightSound();
        }
    }    

    private void Roll()
    { 
        float rollInput = gameActions.Spaceship.Roll.ReadValue<float>();
        rb.maxAngularVelocity = maxAngularVelocity;
        rb.AddRelativeTorque(0, 0, rollInput * rollTorque * Time.deltaTime, ForceMode.Force);        
    }

    private void Thrust()
    {
        bool thrustInput = gameActions.Spaceship.Thrust.ReadValue<float>() > 0;
        if (thrustInput)
        {
            speed = Mathf.Lerp(speed, thrust, Time.deltaTime * 3);
        }
        else
        {
            speed = Mathf.Lerp(speed, 0, Time.deltaTime * 3);
        }
        rb.velocity = transform.forward * speed;
        /*if (thrustInput > 0)
        {
            thrustSlider.value += velocityFactor * Time.deltaTime;
        }
        if (thrustInput < 0)
        {
            thrustSlider.value -= velocityFactor * Time.deltaTime;
        }
        thrustPercent = thrustSlider.value;
        if (!GetComponent<Hull>().colliding)
        {
            rb.AddForce(thrustPercent * thrust * transform.forward * Time.deltaTime, ForceMode.VelocityChange);
            if (rb.velocity.magnitude > maxVelocity)
            {
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
            }
        }*/


    }

    private void UpDown()
    {
        upDownInput = gameActions.Spaceship.UpDown.ReadValue<float>();
        if (!GetComponent<Hull>().isGrounded)
        {
            GetComponent<PlayerGravity>().enabled = false;
            rb.AddRelativeTorque(-upDownInput * upDownForce * Time.deltaTime, 0, 0, ForceMode.Force);
        }
        else if (GetComponent<Hull>().isGrounded && upDownInput > 0 && thrustPercent >= 10)
        {
            GetComponent<PlayerGravity>().enabled = false;
        }
        else if (GetComponent<Hull>().isGrounded && upDownInput < 0 || GetComponent<Hull>().isGrounded && thrustPercent < 10)
        {
            GetComponent<PlayerGravity>().enabled = true;
        }
    }

    private void Strafe()
    {    
        float strafeInput = gameActions.Spaceship.Strafe.ReadValue<float>();
        rb.AddRelativeTorque(0, strafeInput * strafeForce * Time.deltaTime, 0, ForceMode.Force);
    }

    private void FlightSound()
    {
        // Play spaceship flight sound when thrust is higher than 10%
        if (thrustPercent > 10 && Time.time > flySoundStart + audioManager.GetAudioLength("spaceship-sound"))
        {
            audioManager.Play("spaceship-sound");
            flySoundStart = Time.time;
        }
    }
}
