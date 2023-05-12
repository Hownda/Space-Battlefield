using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// The class <c>SpaceshipMovement</c> contains variables and functions that lead to the movement of the spaceship. While the player is not inside the spaceship, this class will be deactivated, so the player movement input only contributes to the player characters transform.
/// </summary>
public class SpaceshipMovementDemo : MonoBehaviour
{
    // Movement Factors
    private float rollTorque = 40000f;
    private float thrust = 50f;
    private float upDownForce = 8000f;
    private float strafeForce = 4000f;
    private float velocityFactor = 15f;

    Rigidbody rb;
    public AudioManagerDemo audioManager;
    public GameObject playerPrefab;
    private GroundManeuvering groundManeuvering;
    private MovementControls gameActions;
    public Slider thrustSlider;

    // Inputs
    private float thrust1D;
    private float strafe1D;
    private float upDown1D;
    private float roll1D;

    public float thrustPercent = 0;
    private float flySoundStart = 0f;

    private void OnEnable()
    {
        gameActions = KeybindManager.inputActions;
        gameActions.GroundMovement.Disable();
        gameActions.Spaceship.Exit.started += Exit;
        gameActions.Spaceship.Enable();

        thrustSlider.gameObject.SetActive(true);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        flySoundStart = -(audioManager.GetAudioLength("spaceship-sound"));
        groundManeuvering = GetComponent<GroundManeuvering>();
    }

    private void FixedUpdate()
    {
        MovementInput();
        FlightSound();
    }

    private void MovementInput()
    {
        Roll();
        Thrust();
        UpDown();
        Strafe();        
    }

    private void Roll()
    {
        Quaternion rotation = transform.rotation;
        rotation *= Quaternion.Euler(roll1D * rollTorque * Time.deltaTime, 0, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * .8f);
    }

    private void Thrust()
    {
        if (thrust1D > 0)
        {
            thrustSlider.value += velocityFactor * Time.deltaTime;
        }
        if (thrust1D < 0)
        {
            thrustSlider.value -= velocityFactor * Time.deltaTime;
        }
        thrustPercent = thrustSlider.value;
        Vector3 spaceshipPosition = transform.position;
        spaceshipPosition += thrustPercent * thrust * Time.deltaTime * transform.right;
        transform.position = Vector3.Lerp(transform.position, spaceshipPosition, Time.deltaTime * .2f);
    }

    private void UpDown()
    {
        if (!groundManeuvering.isGrounded)
        {
            GetComponent<ObjectGravityDemo>().enabled = false;
            Quaternion upDownRotation = transform.rotation;
            upDownRotation *= Quaternion.Euler(0, 0, upDown1D * upDownForce * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, upDownRotation, Time.deltaTime * .2f);
        }
        else if (groundManeuvering.isGrounded && upDown1D >= 0)
        {
            GetComponent<ObjectGravityDemo>().enabled = false;
            Quaternion upDownRotation = transform.rotation;
            upDownRotation *= Quaternion.Euler(0, 0, upDown1D * upDownForce * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, upDownRotation, Time.deltaTime * .2f);
        }
        else if (groundManeuvering.isGrounded && upDown1D < 0 || groundManeuvering.isGrounded && thrustPercent < 10)
        {
            GetComponent<ObjectGravityDemo>().enabled = true;
        }
    }

    private void Strafe()
    {
        Quaternion strafeRotation = transform.rotation;
        strafeRotation *= Quaternion.Euler(0, strafe1D * strafeForce * Time.deltaTime, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, strafeRotation, Time.deltaTime * .2f);
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

    // Since Rigidbody physics are not being used, we need to make sure that we have a way to counteract
    // influences from collisions. A simple method that has been implemented here is to reset the velocity
    // of the Rigidbody when the player gives a movement input. The same has been done for all the rotation inputs.
    public void OnThrust(InputAction.CallbackContext context)
    {
        thrust1D = context.ReadValue<float>();        
        ResetRigidbodyForces("velocity");
    }

    public void OnStrafe(InputAction.CallbackContext context)
    {
        strafe1D = context.ReadValue<float>();
        ResetRigidbodyForces("angularVelocity");   
    }

    public void OnUpDown(InputAction.CallbackContext context)
    {
        upDown1D = context.ReadValue<float>();
        ResetRigidbodyForces("angularVelocity");
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        roll1D = context.ReadValue<float>();
        ResetRigidbodyForces("angularVelocity");
    }

    private void ResetRigidbodyForces(string forceType)
    {
        // Prevents spaceship from glitching into environment
        if (!GetComponent<ObjectGravityDemo>().colliding)
        {
            if (forceType == "angularVelocity")
            {
                // Resets Rigidbody forces
                rb.angularVelocity = Vector3.zero;
            }
            if (forceType == "velocity")
            {
                // Resets Rigidbody forces
                rb.velocity = Vector3.zero;
            }
        }
    }

    // When exiting the spaceship the player character gets spawned above the spaceship.
    // The movement inputs and camera informations will now only tracked on the player.
    private void Exit(InputAction.CallbackContext obj)
    {
        thrustPercent = 0;
        thrustSlider.gameObject.SetActive(false);
        GetComponentInChildren<Camera>().enabled = false;
        GetComponentInChildren<SpaceshipMovementDemo>().enabled = false;
        GetComponentInChildren<PlayerInput>().enabled = false;
        GetComponentInChildren<AudioListener>().enabled = false;
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        Vector3 spawnPosition = transform.position + 3 * ((transform.position - GetComponent<ObjectGravityDemo>().gravityOrbit.transform.position).normalized);
        GameObject player = Instantiate(playerPrefab, new Vector3(spawnPosition.x, spawnPosition.y, spawnPosition.z), Quaternion.Euler(Vector3.zero));

    }
}
