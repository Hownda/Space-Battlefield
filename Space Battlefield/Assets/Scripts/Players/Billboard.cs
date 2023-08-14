using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class Billboard : NetworkBehaviour
{
    public GameObject usernameDisplay;
    public Camera otherCamera;
    private bool allPlayersFound = false;
    private GameObject[] players;

    private void Start()
    {
        if (IsOwner)
        {
            usernameDisplay.SetActive(false);
        }
        allPlayersFound = false;
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
                            player.GetComponent<Billboard>().otherCamera = GetComponentInChildren<Camera>();
                        }
                    }
                    allPlayersFound = true;
                }
            }
        }
        else
        {
            if (otherCamera == null)
            {                
                GameObject localPlayer = null;
                foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
                {
                    if (player.GetComponent<NetworkObject>().OwnerClientId == NetworkManager.LocalClientId)
                    {
                        localPlayer = player;
                        otherCamera = player.GetComponentInChildren<Camera>();
                    }
                }
                if (localPlayer == null)
                {
                    foreach (GameObject spaceship in GameObject.FindGameObjectsWithTag("Spaceship"))
                    {
                        if (spaceship.GetComponent<NetworkObject>().OwnerClientId == NetworkManager.LocalClientId)
                        {
                            otherCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
                        }
                    }
                }
                
            }
            else
            {
                usernameDisplay.transform.parent.LookAt(otherCamera.transform, otherCamera.transform.up);
            }
        }
    }
}