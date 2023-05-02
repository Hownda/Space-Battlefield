using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Billboard : NetworkBehaviour
{
    public Camera cam;

    void Update()
    {
        if (!IsOwner)
        {
            if (cam == null)
            {
                cam = FindObjectOfType<Camera>();
            }
            
            if (cam.enabled == false)
            {
                foreach (KeyValuePair<ulong, GameObject> player in PlayerDictionary.instance.playerDictionary)
                {
                    if (player.Value.GetComponentInChildren<Camera>().enabled == true && player.Value.GetComponentInChildren<Camera>() != null)
                    {
                        cam = player.Value.GetComponentInChildren<Camera>();
                    }
                }
            }

            if (cam == null)
            {
                return;
            }
            transform.LookAt(cam.transform);
        }
    }
}