using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureScalerDemo : MonoBehaviour
{
    public GameObject[] planets;
    private GameObject previousOrbit;

    private void Start()
    {
        planets = GameObject.FindGameObjectsWithTag("Planet");
        previousOrbit = planets[0];
    }

    private void Update()
    {
        // If player is not inside a gravity orbit, mipmap level will be 1 to cover up repetition in the textures.
        if (GetComponentInParent<PlayerGravityDemo>() != null || previousOrbit == null)
        {
            if (GetComponentInParent<PlayerGravityDemo>().gravityOrbit.gameObject != previousOrbit)
            {
                foreach (GameObject planet in planets)
                {
                    if (planet.GetComponentInChildren<GravityOrbitDemo>() == GetComponentInParent<PlayerGravityDemo>().gravityOrbit)
                    {
                        planet.GetComponent<PlanetTexture>().ChangeScaleLevel(0);
                    }
                    else
                    {
                        planet.GetComponent<PlanetTexture>().ChangeScaleLevel(1);
                    }
                }
                previousOrbit = GetComponentInParent<PlayerGravityDemo>().gravityOrbit.gameObject;
            }
        }
        // If spaceship is not inside a gravity orbit, mipmap level will be 1 to cover up repetition in the textures.
        if (GetComponentInParent<ObjectGravityDemo>() != null && GetComponentInParent<SpaceshipMovementDemo>().enabled == true)
        {
            if (GetComponentInParent<ObjectGravityDemo>().gravityOrbit != previousOrbit)
            {
                foreach (GameObject planet in planets)
                {
                    if (planet.GetComponentInChildren<GravityOrbitDemo>() == GetComponentInParent<ObjectGravityDemo>().gravityOrbit)
                    {
                        planet.GetComponent<PlanetTexture>().ChangeScaleLevel(0);
                    }
                    else
                    {
                        planet.GetComponent<PlanetTexture>().ChangeScaleLevel(1);
                    }
                }
                previousOrbit = GetComponentInParent<ObjectGravityDemo>().gravityOrbit.gameObject;
            }
        }
    }
}
