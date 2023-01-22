using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Healthbar : NetworkBehaviour
{
    public int health = 100;

    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health - amount <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GetComponent<NetworkObject>().Despawn(gameObject);
    }
}
