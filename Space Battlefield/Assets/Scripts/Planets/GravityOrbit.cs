using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityOrbit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerGravity>())
        {
            other.GetComponent<PlayerGravity>().gravityOrbit = this.GetComponent<GravityOrbit>();
        }  
        if (other.GetComponentInParent<PlayerGravity>())
        {
            other.GetComponentInParent<PlayerGravity>().gravityOrbit = this.GetComponent<GravityOrbit>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerGravity>())
        {
            other.GetComponent<PlayerGravity>().gravityOrbit = null;
        }
        if (other.GetComponentInParent<PlayerGravity>())
        {
            other.GetComponentInParent<PlayerGravity>().gravityOrbit = null;
        }
    }
}
