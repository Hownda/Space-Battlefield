using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Billboard : NetworkBehaviour
{
    private Camera cam;
    public GameObject billBoardText;

    private void Start()
    {
        UpdateCameraServerRpc();
        if (IsOwner)
        {
            billBoardText.SetActive(false);
        }
    }

    void Update()
    {
        if (!IsOwner)
        {
            transform.LookAt(cam.transform);
        }
    }

    [ServerRpc] public void UpdateCameraServerRpc()
    {
        UpdateCameraClientRpc();
    }   

    [ClientRpc] public void UpdateCameraClientRpc()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<NetworkObject>().OwnerClientId != OwnerClientId)
            {
                player.GetComponentInChildren<Billboard>().cam = transform.parent.GetComponentInChildren<Camera>();
            }
        }
    }

    
}