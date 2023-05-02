using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Commands : MonoBehaviour
{
    public GameObject cube;

    private void Update()
    {
        if (Input.GetKeyDown("r"))
        {
            Instantiate(cube);
        }
    }
}
