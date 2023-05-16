using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Hammer : NetworkBehaviour
{
    public float range;
    private LayerMask naturalResource;
    public float miningPower;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && IsOwner)
        {
            Ray ray = GetComponentInParent<Camera>().ScreenPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, range, naturalResource))
            { 
                if (hit.transform.GetComponent<Resource>())
                {
                    hit.transform.GetComponent<Resource>().Mine(miningPower);
                }
            }
        }
    }
}
