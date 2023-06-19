using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TextureScaler : NetworkBehaviour
{
    private GameObject[] planets;

    void Start()
    {
        planets = GameObject.FindGameObjectsWithTag("Planet");
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            foreach (GameObject planet in planets)
            {
                if (planet.GetComponentInChildren<GravityOrbit>() == GetComponentInParent<PlayerGravity>().gravityOrbit)
                {
                    planet.GetComponent<PlanetTexture>().ChangeScaleLevel(0);
                }
                else
                {
                    planet.GetComponent<PlanetTexture>().ChangeScaleLevel(1);
                }
            }
        }
    }
}
