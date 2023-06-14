using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine;

public class Hull : NetworkBehaviour
{
    public NetworkVariable<int> integrity = new NetworkVariable<int>(100, writePerm: NetworkVariableWritePermission.Server);
    public Slider integritySlider;

    public GameObject integrityBillboard;
    public Camera cam;

    void Start()
    {
       if (IsOwner)
       {
            integritySlider.value = integrity.Value;

            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                if (player.GetComponent<NetworkObject>().OwnerClientId == OwnerClientId)
                {
                    cam = player.GetComponentInChildren<Camera>();
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

    private void SelfDestruct()
    {
        GetComponent<SpaceshipMovement>().Exit();
        Destroy(gameObject);
    }
}
