using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityOrbit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerGravity>())
        {
            other.GetComponent<PlayerGravity>().Gravity = this.GetComponent<GravityOrbit>();
        }

        if (other.transform.GetComponentInParent<ObjectGravity>())
        {
            other.transform.GetComponentInParent<ObjectGravity>().Gravity = this.GetComponent<GravityOrbit>();
        }
    }
}
