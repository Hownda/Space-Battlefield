using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Missile : NetworkBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float speed = 10;
    [SerializeField] private float rotationSpeed = 5;
    [SerializeField] private float maxPredictionTime = 5;
    [SerializeField] private float minDistancePredict = 5;
    [SerializeField] private float maxDistancePredict = 100;
    [SerializeField] private float deviationSpeed = 2;
    [SerializeField] private float deviationAmount = 50;

    public GameObject impactParticles;

    private Vector3 standardPrediction;
    private Vector3 deviatedPrediction;

    public ulong parentClient;
    private GameObject target;
    private bool targetLocked = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetParentClient(ulong clientId)
    {
        parentClient = clientId;
        GameObject[] spaceships = GameObject.FindGameObjectsWithTag("Spaceship");
        GameObject closestTarget = null;
        float closestTargetDistance = 10000;
        foreach (GameObject spaceship in spaceships)
        {
            if (spaceship.GetComponent<NetworkObject>().OwnerClientId != parentClient)
            {
                if (closestTarget == null)
                {
                    closestTarget = spaceship;
                    closestTargetDistance = Vector3.Distance(transform.position, spaceship.transform.position);
                }
                else
                {
                    if (Vector3.Distance(transform.position, spaceship.transform.position) < closestTargetDistance)
                    {
                        closestTarget = spaceship;
                        closestTargetDistance = Vector3.Distance(transform.position, spaceship.transform.position);
                    }
                }
            }
        }
        if (closestTarget == null)
        {
            Debug.Log("No target found");
            Destroy(gameObject);
        }
        else
        {
            target = closestTarget;
            targetLocked = true;
        }
    }

    private void FixedUpdate()
    {
        if (targetLocked)
        {
            if (target != null)
            {
                rb.velocity = transform.forward * speed;

                // Movement prediction
                float leadTimePercentage = Mathf.InverseLerp(minDistancePredict, maxDistancePredict, Vector3.Distance(transform.position, target.transform.position));
                float predictionTime = Mathf.Lerp(0, maxPredictionTime, leadTimePercentage);
                standardPrediction = target.GetComponent<Rigidbody>().position + target.GetComponent<Rigidbody>().velocity * predictionTime;

                // Deviation
                Vector3 deviation = new Vector3(Mathf.Cos(Time.time * deviationSpeed), 0, 0);
                Vector3 predictionOffset = transform.TransformDirection(deviation) * deviationAmount * leadTimePercentage;

                deviatedPrediction = standardPrediction + predictionOffset;

                // Rotate towards target
                Vector3 direction = deviatedPrediction - transform.position;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime));
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsServer)
        {
            if (collision.gameObject.CompareTag("Spaceship"))
            {
                Game.instance.DealDamageToSpaceshipServerRpc(collision.gameObject.GetComponent<NetworkObject>().OwnerClientId, 30);
                GameObject impactEffect = Instantiate(impactParticles, transform.position, Quaternion.Euler(Vector3.zero));
                impactEffect.GetComponentInChildren<ParticleSystem>().Play();
                Destroy(impactEffect, 0.5f);
                Destroy(gameObject);
            }
        }
    }
}
