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

            // Spawns a physical crosshair at location of crosshair on canvas with ScreenToWorldPoint function
            Vector3 crosshairWorldPosition = GetComponentInParent<Camera>().ScreenToWorldPoint(crosshair.transform.position);
            GameObject physicalCrosshair = Instantiate(physicalCrosshairPrefab, transform.parent.parent);
            physicalCrosshair.transform.position = crosshairWorldPosition;

            // Since the ScreenToWorldPoint function returns a way too small value, the physical crosshair
            // is going to need to be moved even more in the direction of the crosshair.
            // For that we subtract the shootingAreaCenter from the crosshair on the canvas to get the exact offset of the crosshair.
            // We can then add that offset to the physicalCrosshair. Since the value is too large it has to be multiplied with a scale factor.
            Vector3 shootingAreaCenter = GetComponentInParent<SpaceshipCamera>().screenCenter;
            physicalCrosshair.transform.localPosition += (new Vector3(0, -crosshair.transform.position.y, crosshair.transform.position.x) - new Vector3(0, -shootingAreaCenter.y, shootingAreaCenter.x)) * scaleFactor;

            // Creates a ray that originates from the physicalCrosshair and passes through the cannon itself.
            // The directions are inverted so we had to use negative y values when adding to the position of the physicalCrosshair.
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
