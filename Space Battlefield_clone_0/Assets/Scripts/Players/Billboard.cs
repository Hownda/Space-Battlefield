using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Billboard : NetworkBehaviour
{
    private Camera cam;

    void Update()
    {
        transform.LookAt(cam.transform);
    }

    public void UpdateCamera(Camera newCamera)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<NetworkObject>().OwnerClientId != OwnerClientId)
            {
                player.GetComponentInChildren<Billboard>().cam = newCamera;
            }
        }
    }

    
}