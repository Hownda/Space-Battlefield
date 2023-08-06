using System.Collections;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine;

public class Hull : NetworkBehaviour
{
    private PlayerNetwork playerNetwork;
    public Camera cam;

    public NetworkVariable<float> integrity = new NetworkVariable<float>(100, writePerm: NetworkVariableWritePermission.Server);
    public Slider integritySlider;
    public Text integrityText;
    public GameObject integrityBillboard;
    public Slider integrityBillboardSlider;
    public Text integrityBillboardText;

    // Collisions
    public GameObject explosionPrefab;
    public bool colliding;
    public float damageFactor = 1;
    public ParticleSystem contactParticles;
    public AudioManager audioManager;
    private float soundStart = 0f;
    private float soundCooldown = 2.5f;
    public GameObject warning;

    public bool isGrounded;
    public LayerMask ground;
    public float distanceFromGround;   

    void Start()
    {
       if (IsOwner)
       {
            integrityBillboard.SetActive(true);
            IgnoreCollisions();
       }
       else
       {
            integritySlider.gameObject.SetActive(false);
       }
    }

    private void IgnoreCollisions()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<NetworkObject>().OwnerClientId != OwnerClientId)
            {
                foreach (Collider collider in GetComponentsInChildren<Collider>())
                {
                    Physics.IgnoreCollision(player.GetComponent<Collider>(), collider);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            isGrounded = Physics.Raycast(transform.position, -transform.up, distanceFromGround, ground);
            RaycastHit hit;
            if (Physics.Raycast(new Ray(transform.position, -transform.up), out hit, distanceFromGround, ground))
            {
                Debug.DrawLine(transform.position, hit.point);
            }
            CheckBounds();
        }
    }

    void Update()
    {       
        integritySlider.value = integrity.Value;
        integrityBillboardSlider.value = integrity.Value;
        integrityText.text = integrity.Value.ToString() + "%";
        integrityBillboardText.text = integrity.Value.ToString() + "%";

        if (cam != null)
        {
            integrityBillboard.transform.LookAt(cam.transform);            
        }

        if (integrity.Value <= 0)
        {
            SelfDestruct();
        }
    }

    public void TakeDamage(float damage)
    {
        integrity.Value -= damage;        
    }

    public void Repair(int amount)
    {
        if (integrity.Value + amount > 100)
        {
            integrity.Value = 100;
        }
        else
        {
            integrity.Value += amount;
        }
        
    }

    private void SelfDestruct()
    {
        GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.Euler(Vector3.zero));
        explosion.GetComponentInChildren<ParticleSystem>().Play();
        Destroy(explosion, 2f);
        audioManager.Play("crash-sound");

        if (PlayerSpawned() == false)
        {
            GetComponent<SpaceshipActions>().Exit();
        }
        Game.instance.RemoveSpaceshipServerRpc(OwnerClientId);
    }

    private bool PlayerSpawned()
    {
        bool playerSpawned = false;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<NetworkObject>().OwnerClientId == OwnerClientId)
            {
                playerSpawned = true;
            }    
        }
        return playerSpawned;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsOwner)
        {
            // Check if player is landing
            if (!isGrounded)
            {
                if (Time.time > soundStart + soundCooldown)
                {
                    if (collision.gameObject.GetComponent<Bullet>() == null)
                    {
                        Impact(collision);
                        audioManager.Play("crash-sound");
                        soundStart = Time.time;                                               
                    }
                }
            }
        }
    }

    private void Impact(Collision collision)
    {
        Vector3 contactPoint = collision.contacts[0].point;
        ParticleSystem particles = Instantiate(contactParticles, gameObject.transform);
        particles.transform.position = contactPoint;
        particles.Play();
        Destroy(particles.gameObject, 1f);

        Game.instance.DealDamageToSpaceshipServerRpc(OwnerClientId, damageFactor);

        
    }

    private void OnCollisionStay(Collision collision)
    {
        
        if (IsOwner)
        {
            if (collision.transform.GetComponent<Rock>())
            {
                collision.transform.GetComponent<Rock>().Mine(5*Time.deltaTime);
            }
            else
            {
                if (!isGrounded)
                {
                    colliding = true;
                    GetComponent<SpaceshipMovement>().upDownInput = 1;
                }
            }
        }
    }
    private void OnCollisionExit()
    {
        if (IsOwner)
        {
            StartCoroutine(EngineActivationDelay());
        }
    }

    private IEnumerator EngineActivationDelay()
    {
        yield return new WaitForSeconds(.5f);
        colliding = false;
        GetComponent<SpaceshipMovement>().upDownInput = 0;
    }

    private void CheckBounds()
    {
        if (transform.position.y < -550 || transform.position.y > 550
            || transform.position.z < -800 || transform.position.z > 800
            || transform.position.x < -750 || transform.position.x > 750)
        {
            Game.instance.DealDamageToSpaceshipServerRpc(OwnerClientId, 0.025f);
            warning.SetActive(true);
        }
        else
        {
            warning.SetActive(false);
        }
    }

}
