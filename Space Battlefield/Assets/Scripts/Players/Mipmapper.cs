using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Mipmapper : NetworkBehaviour
{
    public GameObject[] planets;
    private PlayerGravity playerGravity;

    private int globalMipmap = 0;

    private void Start()
    {
        if (IsOwner)
        {
            planets = GameObject.FindGameObjectsWithTag("Planet");
            playerGravity = GetComponentInParent<PlayerGravity>();
        }
    }

    private void Update()
    {
        if (IsOwner) {
            // If player is not inside a gravity orbit, mipmap level will be 1 to cover up repetition in the textures.
            if (playerGravity.Gravity == null && globalMipmap == 0)
            {
                foreach (GameObject planet in planets)
                {
                    planet.GetComponent<PlanetTexture>().SwapMipmap();
                }
            }
        }
    }
}
