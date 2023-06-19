using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine;

public class Hull : NetworkBehaviour
{
    private PlayerNetwork playerNetwork;

    public NetworkVariable<float> integrity = new NetworkVariable<float>(100, writePerm: NetworkVariableWritePermission.Server);
    public Slider integritySlider;

    public GameObject integrityBillboard;
    public Slider integrityBillboardSlider;
    public Camera cam;

    // Collisions
    public GameObject explosionPrefab;
    private Rigidbody rb;
    public bool colliding;
    public float collisionFactor;
    public float damageFactor = 1;
    public ParticleSystem contactParticles;
    public AudioManager audioManager;
    private float soundStart = 0f;
    private float soundCooldown = 2.5f;

    void Start()
    {
       if (IsOwner)
       {
            rb = GetComponent<Rigidbody>();

            integrityBillboard.SetActive(true);
            integritySlider.value = integrity.Value;
            integrityBillboardSlider.value = integrity.Value;

            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                if (player.GetComponent<NetworkObject>().OwnerClientId == OwnerClientId)
                {
                    cam = player.GetComponentInChildren<Camera>();
                }
            }

            GameObject[] playerRoots = GameObject.FindGameObjectsWithTag("Root");
            foreach (GameObject playerRoot in playerRoots)
            {
                if (playerRoot.GetComponent<NetworkObject>().OwnerClientId == OwnerClientId)
                {
                    playerNetwork = playerRoot.GetComponent<PlayerNetwork>();
                }
            }

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

    void Update()
    {
        integritySlider.value = integrity.Value;
        integrityBillboardSlider.value = integrity.Value;

        if (cam != null)
        {
            integrityBillboard.transform.LookAt(cam.transform);            
        }       
    }

    public void TakeDamage(float damage)
    {
        integrity.Value -= damage;
        if (integrity.Value - damage <= 0)
        {
            SelfDestruct();
        }
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
            GetComponent<SpaceshipMovement>().Exit();
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
            if (!GetComponent<GroundManeuvering>().isGrounded)
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

        float collisionAngle = Vector3.Angle(collision.contacts[0].normal, transform.forward);
        Debug.Log(collisionAngle);
        if (collisionAngle > 120)
        {
            Game.instance.DealDamageToSpaceshipServerRpc(OwnerClientId, 1000);
        }
        else
        {
            // Damage based on impact force
            float collisionForce = rb.velocity.magnitude;
            Game.instance.DealDamageToSpaceshipServerRpc(OwnerClientId, (int)collisionForce * damageFactor);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (IsOwner)
        {
            colliding = true;
        }
    }

    private void OnCollisionExit()
    {
        if (IsOwner)
        {
            colliding = false;
        }
    }
}
