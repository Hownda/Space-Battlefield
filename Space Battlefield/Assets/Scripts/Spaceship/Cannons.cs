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
    public LayerMask trackLayer;
    public float missileForce = 120f;
    private float fireRate = 0.2f;
    private float lastShot;

    public Collider[] colliders;
    public AudioSource shootSound;

    public GameObject trackingRectangle;
    public AudioSource beepSound;
    private bool targetLocked = false;
    private ulong target;
    public float trackingStrength;
    private float beepingTime;
    private float beepingRate;
    private float minBeepingRate = 2f;

    private void Start()
    {
        lastShot = Time.time;
        spaceshipCamera = Camera.main;
        beepingTime = Time.time;
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
            if (ammo == Ammo.Missile)
            {
                Track();
            }
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
                    missile.GetComponent<Bullet>().parentClientId = NetworkObjectId;
                    missile.GetComponent<Bullet>().damage = true;
                    missile.GetComponent<Bullet>().IgnoreCollisions(colliders);
                    missile.GetComponent<Rigidbody>().AddForce(missileForce * missile.transform.forward, ForceMode.Impulse);
                    Destroy(missile, 4f);
                    shootSound.Play();

                    ShootServerRpc(ray.direction);
                }
                else
                {                  
                    SummonMissileServerRpc(NetworkObjectId, targetLocked, target, cannons.transform.position, ray.direction, trackingRectangle.transform.localScale.x);
                    ammo = Ammo.Bullet;
                    trackingRectangle.SetActive(false);
                    crosshair.GetComponent<Image>().enabled = true;
                    Game.instance.abilityDict[Type.Missile].timeStamp = Time.time;
                    Debug.Log("Missile mode inactive");
                }
            }
        }
    }

    private void Track()
    {
        Ray ray = spaceshipCamera.ScreenPointToRay(crosshair.transform.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 50000, trackLayer))
        {
            Vector2 screenPosition = spaceshipCamera.WorldToScreenPoint(hit.transform.position);
            trackingRectangle.transform.position = screenPosition;
            trackingRectangle.transform.localScale -= new Vector3(Time.deltaTime, Time.deltaTime, Time.deltaTime) * 0.5f;
            beepingRate = 0.25f;
            targetLocked = true;
            target = hit.transform.GetComponent<NetworkObject>().NetworkObjectId;
        }
        else
        {
            trackingRectangle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            trackingRectangle.transform.localScale += new Vector3(Time.deltaTime, Time.deltaTime, Time.deltaTime);
            beepingRate = minBeepingRate;
            targetLocked = false;
        }
        float localScale = trackingRectangle.transform.localScale.x;
        localScale = Mathf.Clamp(localScale, 0.5f, 1);
        trackingRectangle.transform.localScale = new Vector3(localScale, localScale, localScale);

        if (beepingTime + beepingRate < Time.time)
        {
            beepingTime = Time.time;
            beepSound.Play();
        }
    }

    [ServerRpc] private void SummonMissileServerRpc(ulong clientId, bool locked, ulong target, Vector3 position, Vector3 direction, float trackingStrength)
    {
        GameObject missile = Instantiate(missilePrefab, position, Quaternion.LookRotation(direction));
        missile.GetComponent<NetworkObject>().Spawn();
        missile.GetComponent<Missile>().parentClient = clientId;
        if (locked)
        {
            missile.GetComponent<Missile>().target = NetworkManager.Singleton.SpawnManager.SpawnedObjects[target].gameObject;
            missile.GetComponent<Missile>().rotationSpeed *=trackingStrength;
        }
        else
        {
            missile.GetComponent<Missile>().target = null;
        }

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
            missile.GetComponent<Bullet>().parentClientId = NetworkObjectId;
            missile.GetComponent<Bullet>().spaceship = true;
            missile.GetComponent<Bullet>().IgnoreCollisions(colliders);
            missile.GetComponent<Rigidbody>().AddForce(missileForce * missile.transform.forward, ForceMode.Impulse);
            Destroy(missile, 4f);
        }
    }
}
