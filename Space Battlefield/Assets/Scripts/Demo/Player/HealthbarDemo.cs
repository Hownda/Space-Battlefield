using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthbarDemo : MonoBehaviour
{
    public int health = 100;
    public Slider healthSlider;

    private void Start()
    {
        healthSlider.value = health;     
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        healthSlider.value = health;
        if (health - damage <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        
    }
}
