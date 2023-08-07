using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SpaceshipMovement : NetworkBehaviour
{
    // Movement Factors
    public float thrust = 5;
    public float thrustFactor = 5;
    public float upDownInput;
    private float speed = 0;

    private float rollTorque = 10000;   
    private float upDownForce = 6000;   
    public float strafeForce = 10000;
    private float maxAngularVelocity = 8;

    Rigidbody rb;

    public AudioManager audioManager;
    private MovementControls gameActions;
    public Slider thrustSlider;
    public GameObject spaceshipCanvas;

    public float thrustPercent = 0;
    public float volumeFactor = 3;

    public GameObject shipLookTarget;
    public AudioSource thrustSound;
    public AudioMixer audioMixer;
    public AudioMixerGroup thrustGroup;
    public AudioMixerGroup otherThrustGroup;

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
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (IsOwner)
        {
            thrustSound.outputAudioMixerGroup = thrustGroup;           
        }
        else
        {
            thrustSound.outputAudioMixerGroup = otherThrustGroup;           
        }
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            Roll();
            Thrust();
            UpDown();
            Strafe();
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
        float thrustInput = gameActions.Spaceship.Thrust.ReadValue<float>();
        if (thrustInput > 0)
        {
            thrustPercent += Time.deltaTime * thrustFactor;
            thrustPercent = Mathf.Clamp(thrustPercent, 0, 100);
            speed = Mathf.Lerp(speed, thrust / 100 * thrustPercent, 0.5f);
        }
        else if (thrustInput == 0)
        {
            thrustPercent -= Time.deltaTime * thrustFactor / 3;
            thrustPercent = Mathf.Clamp(thrustPercent, 0, 100);
            speed = Mathf.Lerp(speed, thrust / 100 * thrustPercent, 0.5f);
        }
        else
        {
            thrustPercent -= Time.deltaTime * thrustFactor;
            thrustPercent = Mathf.Clamp(thrustPercent, 0, 100);
            speed = Mathf.Lerp(speed, thrust / 100 * thrustPercent, 0.5f);
        }
        thrustSlider.value = thrustPercent;
        rb.velocity = transform.forward * speed;

        // Play Sounds
        float desiredThrustVolume = rb.velocity.magnitude * volumeFactor;
        desiredThrustVolume = Mathf.Clamp(desiredThrustVolume, 0.2f, 1.0f);

        float desiredThrustPitch = rb.velocity.magnitude * 0.2f;
        desiredThrustPitch = Mathf.Clamp(desiredThrustPitch, 0.5f, 2f);

        if (audioMixer.GetFloat("ThrustVolume", out float volume) && audioMixer.GetFloat("ThrustPitch", out float pitch))
        {
            audioMixer.SetFloat("ThrustVolume", Mathf.Lerp(volume, desiredThrustVolume, Time.deltaTime * 10));

            audioMixer.SetFloat("ThrustPitch", Mathf.Lerp(pitch, desiredThrustPitch, Time.deltaTime * 1.5f));

            UpdateThrustSoundEffectServerRpc(desiredThrustVolume, desiredThrustPitch);
        }
    }

    private void UpDown()
    {
        upDownInput = gameActions.Spaceship.UpDown.ReadValue<float>();
        if (!GetComponent<Hull>().isGrounded)
        {
            GetComponent<SpaceshipGravity>().enabled = false;
            rb.AddRelativeTorque(-upDownInput * upDownForce * Time.deltaTime, 0, 0, ForceMode.Force);
        }
        else if (GetComponent<Hull>().isGrounded && upDownInput > 0 && thrustPercent >= 10)
        {
            GetComponent<SpaceshipGravity>().enabled = false;
        }
        else if (GetComponent<Hull>().isGrounded && upDownInput < 0 || GetComponent<Hull>().isGrounded && thrustPercent < 10)
        {
            GetComponent<SpaceshipGravity>().enabled = true;
        }
    }

    private void Strafe()
    {    
        float strafeInput = gameActions.Spaceship.Strafe.ReadValue<float>();
        rb.AddRelativeTorque(0, strafeInput * strafeForce * Time.deltaTime, 0, ForceMode.Force);
    }

    [ServerRpc] private void UpdateThrustSoundEffectServerRpc(float volume, float pitch)
    {
        UpdateEngineSoundEffectClientRpc(volume, pitch);
    }

    [ClientRpc]
    private void UpdateEngineSoundEffectClientRpc(float volume, float pitch)
    {
        if (!IsOwner)
        {
            if (audioMixer.GetFloat("OtherThrustVolume", out float currentVolume) && audioMixer.GetFloat("OtherThrustPitch", out float currentPitch))
            {
                audioMixer.SetFloat("OtherThrustVolume", Mathf.Lerp(currentVolume, volume, Time.deltaTime * 10));
                audioMixer.SetFloat("OtherThrustPitch", Mathf.Lerp(currentPitch, pitch, Time.deltaTime * 1.5f));
            }
        }
    }
}
