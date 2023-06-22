using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
    private float health = 100;

    public void Mine(float miningPower)
    {
        health -= miningPower;
        transform.localScale -= new Vector3(0.1f, 0.1f, 0.1f);
        if (health <= 30 || transform.localScale.x <= 0.2f)
        {
            Depleted();
            Destroy(gameObject);
        }
    }

    private void Depleted()
    {

    }
}
