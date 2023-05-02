using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

/// <summary>
/// The class <c>SpaceshipMovement</c> contains variables and functions that lead to the movement of the spaceship. While the player is not inside the spaceship, this class will be deactivated, so the player movement input only contributes to the player characters transform.
/// </summary>
public class SpaceshipMovement : NetworkBehaviour
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
    public float thrustPercent = 0;

    private float flySoundStart = 0f;

    public AudioManager audioManager;
    public GameObject playerPrefab;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        flySoundStart = -(audioManager.GetAudioLength("spaceship-sound"));
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            MovementInput();
            if (thrustPercent > 10 && Time.time > flySoundStart + audioManager.GetAudioLength("spaceship-sound"))
            {
                audioManager.Play("spaceship-sound");
                flySoundStart = Time.time;
            }
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            if (Input.GetKeyDown("f"))
            {
                Exit();
            }
        }
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
            }
            else
            {
                thrustPercent += 100 - thrustPercent;
            }
        }
        else if (thrust1D < 0 && thrustPercent > 0)
        {
            if (thrustPercent - velocityFactor > 0)
            {
                thrustPercent -= velocityFactor * Time.deltaTime;
            }
            else
            {
                thrustPercent -= thrustPercent - 0;
            }
        }
        Vector3 spaceshipPosition = transform.position;
        spaceshipPosition += thrustPercent * thrust * Time.deltaTime * transform.right;
        transform.position = Vector3.Lerp(transform.position, spaceshipPosition, Time.deltaTime * .2f);

        // UpDown
        if (!GetComponent<ObjectGravity>().isGrounded)
        {
            GetComponent<ObjectGravity>().enabled = false;
            Quaternion upDownRotation = transform.rotation;
            upDownRotation *= Quaternion.Euler(0, 0, upDown1D * upDownForce * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, upDownRotation, Time.deltaTime * .2f);
        }
        else if (GetComponent<ObjectGravity>().isGrounded && upDown1D >= 0)
        {
            GetComponent<ObjectGravity>().enabled = false;
            Quaternion upDownRotation = transform.rotation;
            upDownRotation *= Quaternion.Euler(0, 0, upDown1D * upDownForce * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, upDownRotation, Time.deltaTime * .2f);
        }
        else if (GetComponent<ObjectGravity>().isGrounded && upDown1D < 0 || GetComponent<ObjectGravity>().isGrounded && thrustPercent < 10)
        {
            GetComponent<ObjectGravity>().enabled = true;
        }

        // Strafe
        Quaternion strafeRotation = transform.rotation;
        strafeRotation *= Quaternion.Euler(0, strafe1D * strafeForce * Time.deltaTime, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, strafeRotation, Time.deltaTime * .2f);
    }
    public void OnThrust(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            thrust1D = context.ReadValue<float>();
            if (!GetComponent<ObjectGravity>().colliding)
            {
                rb.velocity = Vector3.zero;
            }
        }
    }

    public void OnStrafe(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            strafe1D = context.ReadValue<float>();
            if (!GetComponent<ObjectGravity>().colliding)
            {
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    public void OnUpDown(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            upDown1D = context.ReadValue<float>();
            if (!GetComponent<ObjectGravity>().colliding)
            {
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            roll1D = context.ReadValue<float>();
            if (!GetComponent<ObjectGravity>().colliding)
            {
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    private void Exit()
    {
        thrustPercent = 0;
        GetComponentInChildren<Camera>().enabled = false;
        GetComponentInChildren<SpaceshipMovement>().enabled = false;
        GetComponentInChildren<PlayerInput>().enabled = false;
        GetComponentInChildren<AudioListener>().enabled = false;
        SpawnPlayerServerRpc();
    }

    [ServerRpc] private void SpawnPlayerServerRpc()
    {
        GameObject player = Instantiate(playerPrefab, new Vector3(transform.position.x + 3, transform.position.y, transform.position.z), Quaternion.Euler(Vector3.zero));
        player.GetComponent<NetworkObject>().Spawn();
        player.GetComponent<NetworkObject>().ChangeOwnership(OwnerClientId);

    }

    
}
