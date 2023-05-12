using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Bullet : MonoBehaviour
{
    public GameObject impactParticles;

    private void OnCollisionEnter(Collision collision)
    {
        GameObject impactEffect = Instantiate(impactParticles, transform.position, Quaternion.Euler(Vector3.zero));
        impactEffect.GetComponent<ParticleSystem>().Play();
        Destroy(impactEffect, 0.5f);
        if (collision.gameObject.CompareTag("Player"))
        {
            Game.instance.DealDamageToPlayerServerRpc(collision.body.GetComponent<NetworkObject>().OwnerClientId, 13);

        }
        Destroy(gameObject);
        
    }

    public void IgnoreSelfCollision(Collider ownCollider)
    {
        Physics.IgnoreCollision(GetComponent<Collider>(), ownCollider);
    }
}
