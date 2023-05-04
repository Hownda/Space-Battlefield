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
        if (GetComponentInParent<PlayerGravityDemo>() != null)
        {
            // If player is not inside a gravity orbit, mipmap level will be 1 to cover up repetition in the textures.
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
        else
        {
            // If spaceship is not inside a gravity orbit, mipmap level will be 1 to cover up repetition in the textures.
            if (GetComponentInParent<ObjectGravityDemo>().gravityOrbit.gameObject != previousOrbit)
            {
                foreach (GameObject planet in planets)
                {
                    if (planet.GetComponentInChildren<GravityOrbitDemo>().gameObject == GetComponentInParent<ObjectGravityDemo>().gravityOrbit.gameObject)
                    {
                        planet.GetComponent<PlanetTexture>().ChangeScaleLevel(0);
                    }
                    else
                    {
                        planet.GetComponent<PlanetTexture>().ChangeScaleLevel(1);
                    }
                }
            }
            previousOrbit = GetComponentInParent<ObjectGravityDemo>().gravityOrbit.gameObject;
        }
    }
}
