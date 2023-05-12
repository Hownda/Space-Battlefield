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
    public GameObject dummy;

    public float scaleFactor = 0.1f;
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
            lastShot = Time.time;
            Vector3 crosshairPosition = GetComponentInParent<Camera>().ScreenToWorldPoint(crosshair.transform.position);
            GameObject physicalCrosshair = Instantiate(dummy, transform.parent.parent);
            physicalCrosshair.transform.position = crosshairPosition;

            Vector3 screenCenter = GetComponentInParent<SpaceshipCamera>().screenCenter;

            physicalCrosshair.transform.localPosition += (new Vector3(0, -crosshair.transform.position.y, crosshair.transform.position.x) - new Vector3(0, -screenCenter.y, screenCenter.x)) * scaleFactor;
            Ray directionRay = new Ray(physicalCrosshair.transform.position, transform.position - physicalCrosshair.transform.position);
            GameObject missile = Instantiate(missilePrefab, transform.position, Quaternion.LookRotation(directionRay.direction));
            Destroy(physicalCrosshair);

            missile.GetComponent<Bullet>().IgnoreSelfCollision(spaceshipColliders[0]);
            missile.GetComponent<Bullet>().IgnoreSelfCollision(spaceshipColliders[1]);
            missile.GetComponent<Bullet>().IgnoreSelfCollision(spaceshipColliders[2]);

            missile.GetComponent<Rigidbody>().AddForce(missileForce * missile.transform.forward, ForceMode.Impulse);
            Destroy(missile, 2.5f);

            audioManager.Play("blaster-sound");
            muzzleFlash.Play();
        }
    }
}
