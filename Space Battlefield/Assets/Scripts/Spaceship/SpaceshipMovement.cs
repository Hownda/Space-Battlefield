using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.UI;

public class SpaceshipMovement : NetworkBehaviour
{
    // Movement Factors
    public float rollTorque = 5;
    public float thrust = 5;
    public float upDownForce = 6000f;
    public float strafeForce = 4000f;
    public float velocityFactor = 15f;
    public float maxVelocity;
    public float maxAngularVelocity;

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
    public float upDown1D;
    private float roll1D;

    public float thrustPercent = 0;
    private float flySoundStart = 0f;

    private void OnEnable()
    {
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
            Roll();
            Thrust();
            UpDown();
            Strafe();

            FlightSound();
        }
    }    

    private void Roll()
    {
        //Quaternion rotation = transform.rotation;
        //rotation *= Quaternion.Euler(0 , 0, roll1D * rollTorque * Time.deltaTime);
        //transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * .8f);
        rb.maxAngularVelocity = maxAngularVelocity;
        rb.AddRelativeTorque(0, 0, roll1D * rollTorque * Time.deltaTime, ForceMode.Force);        
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
        if (!GetComponent<Hull>().colliding)
        {
            rb.AddForce(thrustPercent * thrust * transform.forward * Time.deltaTime, ForceMode.VelocityChange);
            if (rb.velocity.magnitude > maxVelocity)
            {
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxVelocity);
            }
        }
    }

    private void UpDown()
    {
        if (!GetComponent<Hull>().isGrounded)
        {
            GetComponent<PlayerGravity>().enabled = false;
            rb.AddRelativeTorque(-upDown1D * upDownForce * Time.deltaTime, 0, 0, ForceMode.Force);
        }
        else if (GetComponent<Hull>().isGrounded && upDown1D >= 0)
        {
            rb.AddRelativeTorque(-upDown1D * upDownForce * Time.deltaTime, 0, 0, ForceMode.Force);
        }
        else if (GetComponent<Hull>().isGrounded && upDown1D < 0 || GetComponent<Hull>().isGrounded && thrustPercent < 10)
        {
            GetComponent<PlayerGravity>().enabled = true;
        }
    }

    private void Strafe()
    {
        //Quaternion strafeRotation = transform.rotation;
        //strafeRotation *= Quaternion.Euler(0, strafe1D * strafeForce * Time.deltaTime, 0);
        //transform.rotation = Quaternion.Slerp(transform.rotation, strafeRotation, Time.deltaTime * .2f);
        rb.AddRelativeTorque(0, strafe1D * strafeForce * Time.deltaTime, 0, ForceMode.Force);
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
        }
    }

    public void OnStrafe(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            strafe1D = context.ReadValue<float>();
        }
    }

    public void OnUpDown(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            upDown1D = context.ReadValue<float>();
        }      
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            roll1D = context.ReadValue<float>();
        }
    }

    public void ExitInput(InputAction.CallbackContext obj)
    {
        if (IsOwner)
        {
            Exit();
        }
    }

    public void Exit()
    {
        GetComponent<Hull>().integrityBillboard.SetActive(true);
        GetComponentInChildren<Camera>().enabled = false;
        GetComponentInChildren<SpaceshipCamera>().enabled = false;
        GetComponentInChildren<SpaceshipMovement>().enabled = false;            
        GetComponentInChildren<AudioListener>().enabled = false;
        GetComponent<Cannons>().enabled = false;                       
        SpawnPlayerServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
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
