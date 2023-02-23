using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class Weapon : NetworkBehaviour
{
    public AudioManager audioManager;
    public ParticleSystem muzzleFlash;
    public Camera fpsCamera;

    public Transform attackPoint;
    public GameObject bulletPrefab;
    public GameObject impactEffect;
    public GameObject projectileSpawn;

    public float bulletForce = 100f;
    private float impulseForce = 10f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Shoot();
        }
    }
    
    private void Shoot()
    {
        if (Input.GetButtonDown("Fire1") && IsOwner)
        {
            Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            Vector3 targetPoint;

            if (Physics.Raycast(ray, out hit))
            {
                targetPoint = hit.point;
                GameObject particle = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                particle.GetComponent<ParticleSystem>().Play();
                Destroy(particle, 2f);

                if (hit.transform.gameObject.GetComponent<Healthbar>() != null)
                {
                    hit.transform.gameObject.GetComponent<Healthbar>().TakeDamage(13);
                }
            }
            else
            {
                GameObject bullet = Instantiate(bulletPrefab, attackPoint.transform.position, Quaternion.Euler(new Vector3(0, 0, 0)));                
                bullet.transform.LookAt(ray.GetPoint(30));
                bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * bulletForce, ForceMode.Impulse);
                Destroy(bullet, 1.5f);
            }

            audioManager.Play("blaster-sound");
            muzzleFlash.Play();
        }
    }
}
