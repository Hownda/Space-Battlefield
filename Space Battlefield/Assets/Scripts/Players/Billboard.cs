using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class Billboard : NetworkBehaviour
{
    [HideInInspector] public Camera otherCamera;
    private bool allPlayersFound = false;
    private GameObject[] players;

    void Update()
    {
        if (allPlayersFound == false)
        {
            if (Game.instance.started.Value == true)
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
        if (!IsOwner)
        {
            transform.LookAt(otherCamera.transform);
        }
    }    
}