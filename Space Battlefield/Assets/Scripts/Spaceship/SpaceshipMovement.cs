using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.VFX;

public class SpaceshipMovement : NetworkBehaviour
{
    private SpaceshipActions spaceshipActions;

    // Movement Factors
    public float thrust = 5;
    public float thrustFactor = 5;
    public float upDownInput;
    private bool applyForce = false;
    private float maxVelocity = 19;

    public float rollTorque = 10000;   
    public float upDownForce = 6000;   
    public float strafeForce = 10000;
    private float maxAngularVelocity = 8;

    Rigidbody rb;
    public Image thrustSlider;
    public GameObject spaceshipCanvas;

    public float volumeFactor = 3;

    public GameObject shipLookTarget;
    public AudioSource thrustSound;
    public AudioMixer audioMixer;
    public AudioMixerGroup thrustGroup;
    public AudioMixerGroup otherThrustGroup;

    public VisualEffect thrustEffect;

    private void OnDisable()
    {
        thrustEffect.SetFloat("Lifetime", 0);
        thrustSlider.fillAmount = 0;
        spaceshipCanvas.SetActive(false);
    }

    private void Start()
    {
        spaceshipActions = GetComponent<SpaceshipActions>();
        rb = GetComponent<Rigidbody>();
        rb.inertiaTensor = rb.inertiaTensor;
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

            if (PlayerData.instance.disableCameraMovement)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }    

    private void Roll()
    { 
        float rollInput = spaceshipActions.gameActions.Spaceship.Roll.ReadValue<float>();
        rb.maxAngularVelocity = maxAngularVelocity;
        rb.AddRelativeTorque(0, 0, rollInput * rollTorque * Time.deltaTime, ForceMode.Force);        
    }

    private void Thrust()
    {
        float thrustInput = spaceshipActions.gameActions.Spaceship.Thrust.ReadValue<float>();
        if (thrustInput > 0)
        {
            applyForce = true;
        }
        else if (thrustInput == 0)
        {
            applyForce = false;
        }
        else
        {
            applyForce = false;
        }

        if (applyForce)
        {
            rb.AddForce(transform.forward * thrust);
        }
        thrustSlider.fillAmount = rb.velocity.magnitude / maxVelocity;
        thrustSlider.fillAmount = Mathf.Clamp(thrustSlider.fillAmount, 0, 100);
        thrustEffect.SetFloat("Lifetime", thrustSlider.fillAmount);

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
        upDownInput = spaceshipActions.gameActions.Spaceship.UpDown.ReadValue<float>();
        if (!GetComponent<Hull>().isGrounded)
        {
            GetComponent<SpaceshipGravity>().enabled = false;
            rb.AddRelativeTorque(-upDownInput * upDownForce * Time.deltaTime, 0, 0, ForceMode.Force);
        }
        else if (GetComponent<Hull>().isGrounded && upDownInput > 0)
        {
            GetComponent<SpaceshipGravity>().enabled = false;
        }
        else if (GetComponent<Hull>().isGrounded && upDownInput < 0)
        {
            GetComponent<SpaceshipGravity>().enabled = true;
        }
    }

    private void Strafe()
    {    
        float strafeInput = spaceshipActions.gameActions.Spaceship.Strafe.ReadValue<float>();
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
