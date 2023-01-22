using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Weapon : MonoBehaviour
{
    public AudioManager audioManager;
    public ParticleSystem muzzleFlash;
    public Camera fpsCamera;

    public Transform attackPoint;
    public GameObject bulletPrefab;

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
        if (Input.GetButtonDown("Fire1"))
        {
            Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            Vector3 targetPoint;

            if (Physics.Raycast(ray, out hit))
            {
                targetPoint = hit.point;
                Debug.Log(hit.transform.gameObject.name);
                if (hit.transform.gameObject.GetComponent<Healthbar>() != null)
                {
                    hit.transform.gameObject.GetComponent<Healthbar>().TakeDamage(13);
                }
                else
                {
                    hit.transform.gameObject.GetComponent<Rigidbody>().AddForce(fpsCamera.transform.forward * impulseForce, ForceMode.Impulse);
                }
            }
            else
            {
                targetPoint = ray.GetPoint(75);
            }

            audioManager.Play("blaster-sound");
            muzzleFlash.Play();

            GameObject bullet = Instantiate(bulletPrefab);
            bullet.transform.position = attackPoint.transform.position;
            bullet.transform.LookAt(targetPoint);
            bullet.GetComponent<BulletScript>().SetDestination(targetPoint);
            bullet.GetComponent<Rigidbody>().AddForce(-transform.forward * bulletForce, ForceMode.Impulse);
           
        }
    }
}
