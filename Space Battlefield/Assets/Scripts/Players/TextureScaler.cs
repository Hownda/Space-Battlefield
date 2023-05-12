using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class TextureScaler : NetworkBehaviour
{
    public GameObject[] planets;
    private GameObject previousOrbit;

    private void Start()
    {
        if (IsOwner)
        {
            planets = GameObject.FindGameObjectsWithTag("Planet");
            previousOrbit = planets[0];
        }
    }

    private void Update()
    {
        if (IsOwner) {
            // If player is not inside a gravity orbit, mipmap level will be 1 to cover up repetition in the textures.
            if (GetComponentInParent<PlayerGravity>() != null || previousOrbit == null)
            {
                if (GetComponentInParent<PlayerGravity>().gravityOrbit.gameObject != previousOrbit)
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
                    previousOrbit = GetComponentInParent<PlayerGravity>().gravityOrbit.gameObject;
                }
            }                          
            // If spaceship is not inside a gravity orbit, mipmap level will be 1 to cover up repetition in the textures.
            if (GetComponentInParent<ObjectGravity>() != null && GetComponentInParent<SpaceshipMovement>().enabled == true)
            {
                if (GetComponentInParent<ObjectGravity>().gravityOrbit != previousOrbit)
                {
                    foreach (GameObject planet in planets)
                    {
                        if (planet.GetComponentInChildren<GravityOrbit>() == GetComponentInParent<ObjectGravity>().gravityOrbit)
                        {
                            planet.GetComponent<PlanetTexture>().ChangeScaleLevel(0);
                        }
                        else
                        {
                            planet.GetComponent<PlanetTexture>().ChangeScaleLevel(1);
                        }
                    }
                    previousOrbit = GetComponentInParent<ObjectGravity>().gravityOrbit.gameObject;
                }
            }                                      
        }
    }
}
