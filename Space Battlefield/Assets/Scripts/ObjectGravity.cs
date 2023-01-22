using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGravity : MonoBehaviour
{
    public GravityOrbit Gravity;
    private Rigidbody rb;
    public float gravity = -20f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        if (Gravity)
        {
            Vector3 gravityUp = (transform.position - Gravity.transform.position).normalized;
            Vector3 localUp = transform.up;

            rb.AddForce(gravityUp * gravity);
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, gravityUp) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 50 * Time.deltaTime);
        }
    }
}
