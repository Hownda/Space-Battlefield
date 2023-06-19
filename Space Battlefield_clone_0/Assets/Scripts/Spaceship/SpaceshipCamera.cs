using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class SpaceshipCamera : NetworkBehaviour
{
    public GameObject overlay;

    private void OnEnable()
    {
        overlay.SetActive(true);
    }

    private void OnDisable()
    {
        overlay.SetActive(false);
    }
}
