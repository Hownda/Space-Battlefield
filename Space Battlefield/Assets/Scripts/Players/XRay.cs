using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class XRay : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
       if (IsOwner)
       {
            // Enables X-Ray to track spaceship
            Transform[] childObjects = GetComponentsInChildren<Transform>();
            foreach (Transform childObject in childObjects)
            {
                childObject.gameObject.layer = 10;
            }
       }
    }
}
