using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityOrbitDemo : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerGravityDemo>())
        {
            other.GetComponent<PlayerGravityDemo>().gravityOrbit = this.GetComponent<GravityOrbitDemo>();
        }

        if (other.transform.GetComponentInParent<ObjectGravityDemo>())
        {
            other.transform.GetComponentInParent<ObjectGravityDemo>().gravityOrbit = this.GetComponent<GravityOrbitDemo>();
        }
    }
}
