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

    public GameObject bulletPrefab;

    public float bulletForce = 100f;
    private float fireRate = 0.2f;
    private float lastShot;

    private void Start()
    {
        lastShot = Time.time;  
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0) && Options.instance.disableCameraMovement == false)
        {
            Shoot();
        }
    }
    
    private void Shoot()
    {
        if (Input.GetButtonDown("Fire1") && IsOwner)
        {
            if (Time.time - lastShot > fireRate)
            {
                lastShot = Time.time;
                GameObject bullet = Instantiate(bulletPrefab, fpsCamera.transform.position, Quaternion.Euler(Vector3.zero));
                bullet.GetComponent<Bullet>().IgnoreSelfCollision(GetComponentInParent<CapsuleCollider>());
                bullet.transform.rotation = Quaternion.LookRotation(fpsCamera.transform.forward);
                bullet.GetComponent<Rigidbody>().AddForce(bulletForce * -transform.forward, ForceMode.Impulse);
                Destroy(bullet, 2.5f);

                audioManager.Play("blaster-sound");
                muzzleFlash.Play();
            }
        }
    }
}
