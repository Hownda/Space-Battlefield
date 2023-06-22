using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Bullet : MonoBehaviour
{
    public GameObject impactParticles;
    public ulong parentClient;

    private void OnCollisionEnter(Collision other)
    {   
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.gameObject.GetComponent<NetworkObject>().OwnerClientId != parentClient)
            {
                Game.instance.DealDamageToPlayerServerRpc(other.gameObject.GetComponent<NetworkObject>().OwnerClientId, 13);
                Impact();
            }
        } 
        else if (other.gameObject.CompareTag("Spaceship"))
        {
            if (other.gameObject.GetComponent<NetworkObject>().OwnerClientId != parentClient)
            {
                Game.instance.DealDamageToSpaceshipServerRpc(other.gameObject.GetComponent<NetworkObject>().OwnerClientId, 2);
                
            }            
            Impact();
        }
        else
        {
            Impact();
        }
    }  
    
    private void Impact()
    {
        GameObject impactEffect = Instantiate(impactParticles, transform.position, Quaternion.Euler(Vector3.zero));
        impactEffect.GetComponent<ParticleSystem>().Play();
        Destroy(impactEffect, 0.5f);
        Destroy(gameObject);
    }

    public void IgnoreCollisions(Collider[] colliders)
    {
        foreach (Collider otherCollider in colliders)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), otherCollider);
        }
    }
}
