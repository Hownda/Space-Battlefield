using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
    private float health = 100;

    public void Mine(float miningPower)
    {
        health -= miningPower;
        if (health <= 0)
        {
            Depleted();
        }
    }

    private void Depleted()
    {

    }
}
