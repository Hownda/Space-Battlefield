using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.UI;

public class SpaceshipMovement : NetworkBehaviour
{
    // Movement Factors
    private float rollTorque = 40000f;
    private float thrust = 30f;
    private float upDownForce = 6000f;
    private float strafeForce = 4000f;
    private float velocityFactor = 15f;

    Rigidbody rb;
    public GameObject playerPrefab;

    public AudioManager audioManager;
    private GroundManeuvering groundManeuvering;
    private MovementControls gameActions;
    public Slider thrustSlider;
    public GameObject spaceshipCanvas;

    // Inputs
    private float thrust1D;
    private float strafe1D;
    private float upDown1D;
    private float roll1D;

    public float thrustPercent = 0;
    private float flySoundStart = 0f;

    private void OnEnable()
    {
        // Reset spaceship controls
        gameActions = KeybindManager.inputActions;
        gameActions.Spaceship.Exit.started += ExitInput;
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
        groundManeuvering = GetComponent<GroundManeuvering>();
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            Movement();
            FlightSound();
        }
    }    

    private void Movement()
    {
        Roll();
        Thrust();
        UpDown();
        Strafe();       
    }

    private void Roll()
    {
        Quaternion rotation = transform.rotation;
        rotation *= Quaternion.Euler(0 , 0, roll1D * rollTorque * Time.deltaTime);
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
        spaceshipPosition += thrustPercent * thrust * Time.deltaTime * transform.forward;
        transform.position = Vector3.Lerp(transform.position, spaceshipPosition, Time.deltaTime * .2f);
    }

    private void UpDown()
    {
        if (!groundManeuvering.isGrounded)
        {
            GetComponent<PlayerGravity>().enabled = false;
            Quaternion upDownRotation = transform.rotation;
            upDownRotation *= Quaternion.Euler(-upDown1D * upDownForce * Time.deltaTime, 0, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, upDownRotation, Time.deltaTime * .2f);
        }
        else if (groundManeuvering.isGrounded && upDown1D >= 0)
        {
            GetComponent<PlayerGravity>().enabled = false;
            Quaternion upDownRotation = transform.rotation;
            upDownRotation *= Quaternion.Euler(-upDown1D * upDownForce * Time.deltaTime, 0, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, upDownRotation, Time.deltaTime * .2f);
        }
        else if (groundManeuvering.isGrounded && upDown1D < 0 || groundManeuvering.isGrounded && thrustPercent < 10)
        {
            GetComponent<PlayerGravity>().enabled = true;
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

    public void OnThrust(InputAction.CallbackContext context)
    { 
        if (IsOwner)
        {
            thrust1D = context.ReadValue<float>();
            ResetRigidbodyForces("velocity");
        }
    }

    public void OnStrafe(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            strafe1D = context.ReadValue<float>();
            ResetRigidbodyForces("angularVelocity");
        }
    }

    public void OnUpDown(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            upDown1D = context.ReadValue<float>();
            ResetRigidbodyForces("angularVelocity");
        }      
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            roll1D = context.ReadValue<float>();
            ResetRigidbodyForces("angularVelocity");
        }
    }

    // Since Rigidbody physics are not being used, we need to make sure that we have a way to counteract
    // influences from collisions. A simple method that has been implemented here is to reset the velocity
    // of the Rigidbody when the player gives a movement input. The same has been done for all the rotation inputs.
    public void ResetRigidbodyForces(string forceType)
    {
        // Prevents spaceship from glitching into environment
        if (!GetComponent<CollisionManager>().colliding)
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

    public void ExitInput(InputAction.CallbackContext obj)
    {
        Exit();
    }

    public void Exit()
    {
        if (IsOwner)
        {           
            GetComponentInChildren<Camera>().enabled = false;
            GetComponentInChildren<SpaceshipCamera>().enabled = false;
            GetComponentInChildren<SpaceshipMovement>().enabled = false;            
            GetComponentInChildren<AudioListener>().enabled = false;
            GetComponentInChildren<TextureScaler>().enabled = false;
            GetComponentInChildren<Cannons>().enabled = false;            
            SpawnPlayerServerRpc();
        }
    }

    [ServerRpc]
    private void SpawnPlayerServerRpc()
    {
        Vector3 spawnPosition;
        if (GetComponent<PlayerGravity>().gravityOrbit != null)
        {
            spawnPosition = transform.position + 3 * ((transform.position - GetComponent<PlayerGravity>().gravityOrbit.transform.position).normalized);
        }
        else
        {
            spawnPosition = transform.position + 3 * transform.up;
        }
        GameObject player = Instantiate(playerPrefab, new Vector3(spawnPosition.x, spawnPosition.y, spawnPosition.z), Quaternion.Euler(Vector3.zero));
        player.GetComponent<NetworkObject>().Spawn();
        player.GetComponent<NetworkObject>().ChangeOwnership(OwnerClientId);

        GetComponent<Hull>().cam = player.GetComponentInChildren<Camera>();
    }
}
