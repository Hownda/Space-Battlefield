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

    public Animator animator;
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
            if (PlayerData.instance.disableCameraMovement == false)
            {
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    Shoot();
                    handAnimator.SetBool("Shoot", true);
                    AnimateServerRpc(true);

                }
                if (Input.GetKeyUp(KeyCode.Mouse0))
                {
                    handAnimator.SetBool("Shoot", false);
                    AnimateServerRpc(false);
                }
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
                bullet.GetComponent<Bullet>().parentClientId = NetworkObjectId;
                bullet.GetComponent<Bullet>().damage = true;
                bullet.GetComponent<Bullet>().IgnoreCollisions(colliders);
                bullet.transform.rotation = Quaternion.LookRotation(fpsCamera.transform.forward);
                bullet.GetComponent<Rigidbody>().AddForce(bulletForce * bullet.transform.forward, ForceMode.Impulse);
                Destroy(bullet, 2.5f);

                audioManager.Play("blaster-sound");
                muzzleFlash.Play();

                ShootServerRpc(fpsCamera.transform.forward);
            }
        }
    }

    [ServerRpc] private void ShootServerRpc(Vector3 shootDirection)
    {
        ShootClientRpc(shootDirection);
    }

    [ClientRpc] private void ShootClientRpc(Vector3 shootDirection)
    {
        if (!IsOwner)
        {
            GameObject bullet = Instantiate(bulletPrefab, fpsCamera.transform.position, Quaternion.Euler(Vector3.zero));
            bullet.GetComponent<Bullet>().parentClientId = NetworkObjectId;
            bullet.GetComponent<Bullet>().IgnoreCollisions(colliders);
            bullet.transform.rotation = Quaternion.LookRotation(shootDirection);
            bullet.GetComponent<Rigidbody>().AddForce(bulletForce * bullet.transform.forward, ForceMode.Impulse);
            Destroy(bullet, 2.5f);
        }
    }

    [ServerRpc] private void AnimateServerRpc(bool shoot)
    {
        AnimateClientRpc(shoot);
    }

    [ClientRpc] private void AnimateClientRpc(bool shoot)
    {
        if (!IsOwner)
        {
            animator.SetBool("Shoot", shoot);
        }
    }
}
