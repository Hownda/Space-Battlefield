using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundManeuvering : MonoBehaviour
{
    public bool isGrounded;
    public LayerMask ground;
    public float distanceFromGround;

    private void Update()
    {
        isGrounded = Physics.Raycast(transform.position, -transform.up, distanceFromGround, ground);
        Debug.DrawRay(transform.position, -transform.up);
    }
}
