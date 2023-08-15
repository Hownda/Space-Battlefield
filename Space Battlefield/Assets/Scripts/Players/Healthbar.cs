using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class Healthbar : NetworkBehaviour
{
    public NetworkVariable<int> health = new NetworkVariable<int>(100, writePerm:NetworkVariableWritePermission.Server);
    public Slider healthSlider;

    private void Start()
    {
        if (IsOwner)
        {
            healthSlider.value = health.Value;
        }
        else
        {
            healthSlider.gameObject.SetActive(false);
        }
        
    }

    public void Update()
    {
        healthSlider.value = health.Value;
    }

    public void TakeDamage(int damage)
    {
        health.Value -= damage;    
    }

    public void Heal(int amount)
    {
        if (health.Value + amount > 100)
        {
            health.Value = 100;
        }
        else
        {
            health.Value += amount;
        }
    }
}
