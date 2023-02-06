using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveCrosshair : MonoBehaviour
{
    [SerializeField]
    private float movementFactor = 1;
    public void HorizontalInteraction(float movementInput)
    {
        transform.position += transform.right * movementFactor * movementInput;
    }
    public void ResetInteraction()
    {
        if (transform.position != Vector3.zero)
        {

        }
    }
}
