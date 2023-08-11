using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class Billboard : NetworkBehaviour
{
    [HideInInspector] public Camera otherCamera;
    private bool allPlayersFound = false;
    private GameObject[] players;

    private void Start()
    {
        if (IsOwner)
        {
            GetComponentInChildren<Text>().enabled = false;
        }
    }

    void Update()
    {
        if (IsOwner)
        {
            if (allPlayersFound == false)
            {
                if (Game.instance.GetComponent<GameEvents>().started.Value == true)
                {
                    players = GameObject.FindGameObjectsWithTag("Player");
                    foreach (GameObject player in players)
                    {
                        if (player.GetComponent<NetworkObject>().OwnerClientId != OwnerClientId)
                        {
                            player.GetComponentInChildren<Billboard>().otherCamera = GetComponentInChildren<Camera>();
                        }
                    }
                    allPlayersFound = true;
                }
            }
        }
        else
        {
            if (otherCamera != null)
            {
                Quaternion lookRotation = Quaternion.LookRotation(otherCamera.transform.position - transform.position, transform.up);
                transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, lookRotation.eulerAngles.y, transform.rotation.eulerAngles.z));
            }
        }
    }    
}