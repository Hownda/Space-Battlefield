using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

/// <summary>
/// Contains all variables and functions of the first person weapon.
/// </summary>
public class Weapon : NetworkBehaviour
{
    public AudioManager audioManager;
    public ParticleSystem muzzleFlash;
    public Camera fpsCamera;

    public Animator handAnimator;

    public GameObject bulletPrefab;

    public float bulletForce = 100f;
    private float fireRate = 0.2f;
    private float lastShot;

    public Collider[] colliders;

    private void Start()
    {
        lastShot = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            if (Input.GetKey(KeyCode.Mouse0) && Options.instance.disableCameraMovement == false)
            {
                Shoot();
                handAnimator.SetBool("Shoot", true);
            }
            if (Input.GetKeyUp(KeyCode.Mouse0) && Options.instance.disableCameraMovement == false)
            {
                handAnimator.SetBool("Shoot", false);
            }
        }
    }
    
    private void Shoot()
    {
        if (IsOwner)
        {
            if (Time.time - lastShot > fireRate)
            {
                lastShot = Time.time;
                GameObject bullet = Instantiate(bulletPrefab, fpsCamera.transform.position, Quaternion.Euler(Vector3.zero));
                bullet.GetComponent<Bullet>().parentClient = OwnerClientId;
                bullet.GetComponent<Bullet>().IgnoreCollisions(colliders);
                bullet.transform.rotation = Quaternion.LookRotation(fpsCamera.transform.forward);
                bullet.GetComponent<Rigidbody>().AddForce(bulletForce * bullet.transform.forward, ForceMode.Impulse);
                Destroy(bullet, 2.5f);

                audioManager.Play("blaster-sound");
                muzzleFlash.Play();
            }
        }
    }
}
