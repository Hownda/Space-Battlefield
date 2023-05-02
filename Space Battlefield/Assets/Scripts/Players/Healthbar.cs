using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class Healthbar : NetworkBehaviour
{
    public int health = 100;
    public Slider healthSlider;

    private void Start()
    {
        if (IsOwner)
        {
            healthSlider.value = health;
        }
        else
        {
            healthSlider.gameObject.SetActive(false);
        }
        
    }

    public void TakeDamage(int damage)
    {
        if (IsOwner)
        {
            health -= damage;
            healthSlider.value = health;
            if (health - damage <= 0)
            {
                Die();
            }
        }
    }

    private void Die()
    {
        GetComponent<NetworkObject>().Despawn(gameObject);
    }
}
