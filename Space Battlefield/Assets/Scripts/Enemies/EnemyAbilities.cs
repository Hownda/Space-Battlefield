using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public enum EnemyAbilityType
{
    Shield, RangedAttack, Charge
}

public class EnemyAbility
{
    public EnemyAbilityType type;
    public float cooldown;
    public float usageTime;
    public float duration;
    public bool active;

    public EnemyAbility(EnemyAbilityType type, float cooldown, float usageTime, float duration, bool active)
    {
        this.type = type;
        this.cooldown = cooldown;
        this.usageTime = usageTime;
        this.duration = duration;
        this.active = active;
    }
}

public class EnemyAbilities : MonoBehaviour
{
    private AlienDog alienDog;
    private Dictionary<EnemyAbilityType, EnemyAbility> abilities = new() { { EnemyAbilityType.Shield, new EnemyAbility(EnemyAbilityType.Shield, 10, 0, 10, false) }, 
        { EnemyAbilityType.RangedAttack, new EnemyAbility(EnemyAbilityType.RangedAttack, 5, 0, 2, false) }, 
        { EnemyAbilityType.Charge, new EnemyAbility(EnemyAbilityType.Charge, 15, 0, 3, false) } };

    private float attackTime;
    private float attackCooldown = 2f;
    private float attackRange = 5;

    //Ranged Attack
    public GameObject projectilePrefab;
    public float firingAngle = 45;

    private void Start()
    {
        alienDog = GetComponent<AlienDog>();
        attackTime = Time.time;
        foreach (KeyValuePair<EnemyAbilityType, EnemyAbility> ability in abilities)
        {
            ability.Value.usageTime = Time.time;
        }
    }

    private void Update()
    {
        Attack();
    }

    private void Attack()
    {
        if (alienDog.player != null)
        {
            if (Vector3.Distance(transform.position, alienDog.player.transform.position) <= attackRange)
            {
                if (alienDog.state != AlienDog.State.Attacking)
                {
                    alienDog.state = AlienDog.State.Attacking;
                    alienDog.ChangeStateClientRpc(alienDog.state);
                    alienDog.speed = alienDog.slowSpeed;
                }
                if (attackTime + attackCooldown < Time.time)
                {
                    alienDog.animator.SetTrigger("Attack");
                    alienDog.PlayAnimationClientRpc("Attack");
                    attackTime = Time.time;
                    StartCoroutine(DamageDelay());
                }
            }
            else
            {
                if (alienDog.player.CompareTag("Spaceship"))
                {
                    RangedAttack();
                }
                if (alienDog.state != AlienDog.State.Aggressive)
                {
                    alienDog.state = AlienDog.State.Aggressive;
                    alienDog.ChangeStateClientRpc(alienDog.state);
                    alienDog.speed = alienDog.fastSpeed;
                }
            }
        }
    }

    private void RangedAttack()
    {
        EnemyAbility rangedAttack = abilities[EnemyAbilityType.RangedAttack];
        if (rangedAttack.usageTime + rangedAttack.cooldown < Time.time)
        {
            alienDog.animator.SetTrigger("Attack");
            alienDog.PlayAnimationClientRpc("Attack");
            rangedAttack.usageTime = Time.time;

            StartCoroutine(SummonMissile());           
        }
    }

    private IEnumerator SummonMissile()
    {
        yield return new WaitForSeconds(0.5f);
        GameObject projectile = Instantiate(projectilePrefab, transform.position + alienDog.GetGravityUp() * 2, Quaternion.LookRotation(alienDog.player.transform.position - transform.position));
        projectile.GetComponent<NetworkObject>().Spawn();
        projectile.GetComponent<Missile>().parentClient = 999;
        projectile.GetComponent<Missile>().target = alienDog.player;

        Physics.IgnoreCollision(GetComponent<Collider>(), projectile.GetComponent<Collider>());
    }

    private IEnumerator DamageDelay()
    {
        yield return new WaitForSeconds(.5f);
        Debug.Log("Attack");
        if (Vector3.Distance(transform.position, alienDog.player.transform.position) <= attackRange)
        {
            if (alienDog.player.CompareTag("Player"))
            {
                Game.instance.DealDamageToPlayerServerRpc(alienDog.player.GetComponent<NetworkObject>().OwnerClientId, 15, 999);                
            }
            else
            {
                Game.instance.DealDamageToSpaceshipServerRpc(alienDog.player.GetComponent<NetworkObject>().OwnerClientId, 15, 999);
            }
            alienDog.punchSound.Play();
            alienDog.PlayAttackClientRpc();
        }
    }
}
