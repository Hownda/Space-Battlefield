using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Cannons : NetworkBehaviour
{
    public AudioManager audioManager;
    public ParticleSystem muzzleFlash;

    public GameObject missilePrefab;
    public GameObject crosshair;

    public Collider[] spaceshipColliders;
    public GameObject physicalCrosshairPrefab;

    private float scaleFactor = 0.02f;
    public float missileForce = 100f;
    private float fireRate = 0.2f;
    private float lastShot;

    private void Start()
    {
        lastShot = Time.time;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0) && IsOwner && Options.instance.disableCameraMovement == false)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (Time.time - lastShot > fireRate)
        {
            // Start Cooldown
            lastShot = Time.time;

            GameObject missile = Instantiate(missilePrefab, transform.position, Quaternion.LookRotation(transform.forward));

            missile.transform.position = transform.position;
            missile.GetComponent<Bullet>().parentClient = OwnerClientId;
            missile.GetComponent<Rigidbody>().AddForce(missileForce * missile.transform.forward, ForceMode.Impulse);
            Destroy(missile, 2.5f);

            audioManager.Play("blaster-sound");
            muzzleFlash.Play();
        }
    }
}
