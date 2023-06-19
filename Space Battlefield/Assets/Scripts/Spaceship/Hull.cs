using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine;

public class Hull : NetworkBehaviour
{
    private PlayerNetwork playerNetwork;

    public NetworkVariable<int> integrity = new NetworkVariable<int>(100, writePerm: NetworkVariableWritePermission.Server);
    public Slider integritySlider;

    public GameObject integrityBillboard;
    public Slider integrityBillboardSlider;
    public Camera cam;

    void Start()
    {
       if (IsOwner)
       {
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
       }
       else
       {
            integritySlider.gameObject.SetActive(false);
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

    public void TakeDamage(int damage)
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
        GetComponent<SpaceshipMovement>().Exit();
        Destroy(gameObject);
    }
}
