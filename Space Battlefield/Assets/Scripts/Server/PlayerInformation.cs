using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInformation
{
    public ulong clientId;
    public GameObject root;
    public GameObject player;
    public GameObject spaceship;

    public PlayerInformation(ulong clientId, GameObject root, GameObject player, GameObject spaceship)
    {
        this.clientId = clientId;
        this.root = root;
        this.player = player;
        this.spaceship = spaceship;
    }
}
