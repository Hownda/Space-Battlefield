using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public enum Ammo
{
    Bullet, Missile
}

public class Cannons : NetworkBehaviour
{
    private Camera spaceshipCamera;
    public Ammo ammo;

    public RectTransform crosshair;
    public Vector2 screenCenter;
    public float crosshairSensitivity = 10;
    public float crosshairAreaRadius = 100;

    public GameObject bulletPrefab;
    public GameObject missilePrefab;
    public GameObject cannons;
    public LayerMask layer;
    public float missileForce = 100f;
    private float fireRate = 0.2f;
    private float lastShot;

    public Collider[] colliders;
    public AudioSource shootSound;

    public GameObject trackingRectangle;
    public float trackingFactor = 0.5f;
    public float trackingStrength;

    private void Start()
    {
        lastShot = Time.time;
        spaceshipCamera = Camera.main;
    }

    void Update()
    {
        if (IsOwner)
        {
            CrosshairMovement();
            if (Input.GetKey(KeyCode.Mouse0) && PlayerData.instance.disableCameraMovement == false)
            {
                Shoot();
            }
            /*if (ammo == Ammo.Missile)
            {
                trackingRectangle.transform.localScale += new Vector3(Time.deltaTime * trackingFactor, Time.deltaTime * trackingFactor, Time.deltaTime * trackingFactor);
            }*/
        }
    }

    private void CrosshairMovement()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Vector2 moveVector = new Vector2(mouseX * Time.deltaTime * crosshairSensitivity, mouseY * Time.deltaTime * crosshairSensitivity);
        crosshair.anchoredPosition = crosshair.anchoredPosition + moveVector;
        crosshair.anchoredPosition = Vector2.ClampMagnitude(crosshair.anchoredPosition /*- screenCenter*/, crosshairAreaRadius) /*+ screenCenter*/;
    }

    private void Shoot()
    {
        if (Time.time - lastShot > fireRate)
        {
            // Start Cooldown
            lastShot = Time.time;

            Ray ray = spaceshipCamera.ScreenPointToRay(crosshair.transform.position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 50000, layer))
            {
                if (ammo == Ammo.Bullet)
                {
                    GameObject missile = Instantiate(bulletPrefab, cannons.transform.position, Quaternion.LookRotation(ray.direction));
                    missile.GetComponent<Bullet>().parentClient = OwnerClientId;
                    missile.GetComponent<Bullet>().damage = true;
                    missile.GetComponent<Bullet>().IgnoreCollisions(colliders);
                    missile.GetComponent<Rigidbody>().AddForce(missileForce * missile.transform.forward, ForceMode.Impulse);
                    Destroy(missile, 2.5f);
                    shootSound.Play();

                    ShootServerRpc(ray.direction);
                }
                else
                {                  
                    SummonMissileServerRpc(OwnerClientId, cannons.transform.position, ray.direction);
                    ammo = Ammo.Bullet;
                    trackingRectangle.SetActive(false);
                    Debug.Log("Missile mode inactive");
                }
            }
        }
    }

    [ServerRpc] private void SummonMissileServerRpc(ulong clientId, Vector3 position, Vector3 direction)
    {
        GameObject missile = Instantiate(missilePrefab, position, Quaternion.LookRotation(direction));
        missile.GetComponent<NetworkObject>().Spawn();
        missile.GetComponent<Missile>().SetParentClient(clientId);

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            Physics.IgnoreCollision(collider, missile.GetComponent<Collider>());
        }
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 shootDirection)
    {
        ShootClientRpc(shootDirection);
    }

    [ClientRpc]
    private void ShootClientRpc(Vector3 shootDirection)
    {
        if (!IsOwner)
        {
            GameObject missile = Instantiate(bulletPrefab, cannons.transform.position, Quaternion.LookRotation(shootDirection));
            missile.GetComponent<Bullet>().parentClient = OwnerClientId;
            missile.GetComponent<Bullet>().IgnoreCollisions(colliders);
            missile.GetComponent<Rigidbody>().AddForce(missileForce * missile.transform.forward, ForceMode.Impulse);
            Destroy(missile, 2.5f);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        /*if (other.CompareTag("Spaceship"))
        {
            float scale = trackingRectangle.transform.localScale.x;
            scale += Time.deltaTime * trackingFactor * 2;
            scale = Mathf.Clamp(scale, 0.5f, 1);
            trackingRectangle.transform.localScale -= new Vector3(Time.deltaTime * trackingFactor * 2, Time.deltaTime * trackingFactor * 2, Time.deltaTime * trackingFactor * 2);
        }*/
    }
}
