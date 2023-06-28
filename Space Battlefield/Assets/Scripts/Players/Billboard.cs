using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Billboard : NetworkBehaviour
{
    public Camera ownCamera;
    private Camera otherCamera;
    public GameObject billBoardText;
    private PlayerNetwork otherPlayer;

    private void Start()
    {
        if (IsOwner)
        {
            billBoardText.SetActive(false);
            
            otherPlayer = PlayerDictionary.instance.GetOtherPlayer(OwnerClientId).GetComponent<PlayerNetwork>();
        }
    }

    void Update()
    {
        if (otherPlayer != null)
        {
            if (otherPlayer.playerObject != null)
            {
                otherPlayer.playerObject.GetComponentInChildren<Billboard>().otherCamera = ownCamera;
            }
        }
        if (otherCamera != null)
        {
            transform.LookAt(otherCamera.transform);
        }
    }    
}