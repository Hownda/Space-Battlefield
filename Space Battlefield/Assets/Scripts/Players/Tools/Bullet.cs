using UnityEngine;
using Unity.Netcode;
using UnityEngine.VFX;

public class Bullet : MonoBehaviour
{
    public GameObject impactParticles;
    public ulong parentClient;
    public bool damage = false;
    private float explosionRadius = 2;

    private void OnCollisionEnter(Collision other)
    {
        Impact();
        Collider[] collisions = Physics.OverlapSphere(transform.position, explosionRadius);
        if (collisions.Length > 0)
        {
            foreach (Collider collision in collisions)
            {
                if (collision.gameObject.CompareTag("Player"))
                {
                    if (damage == true)
                    {
                        if (collision.gameObject.GetComponent<NetworkObject>().OwnerClientId != parentClient)
                        {
                            Game.instance.DealDamageToPlayerServerRpc(collision.gameObject.GetComponent<NetworkObject>().OwnerClientId, 13, parentClient);                            
                        }
                    }
                }
                if (collision.gameObject.CompareTag("Spaceship"))
                {
                    if (damage == true)
                    {
                        if (collision.gameObject.GetComponent<NetworkObject>().OwnerClientId != parentClient)
                        {
                            if (collision.GetComponent<SpaceshipActions>().shieldActive.Value == false)
                            {
                                Game.instance.DealDamageToSpaceshipServerRpc(collision.gameObject.GetComponent<NetworkObject>().OwnerClientId, 2, parentClient);
                            }

                        }
                    }
                }
                if (collision.gameObject.CompareTag("Enemy"))
                {
                    if (damage == true)
                    {
                        Game.instance.DealDamageToEnemyServerRpc(collision.GetComponent<NetworkObject>().NetworkObjectId, 2, parentClient);
                    }
                }
                
            }
        }
    }
    
    private void Impact()
    {
        GameObject impactEffect = Instantiate(impactParticles, transform.position, Quaternion.Euler(Vector3.zero));
        impactEffect.GetComponent<VisualEffect>().Play();
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
