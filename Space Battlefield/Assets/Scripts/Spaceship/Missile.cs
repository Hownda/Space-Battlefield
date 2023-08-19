using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Missile : NetworkBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float speed = 10;
    public float rotationSpeed = 5;
    [SerializeField] private float maxPredictionTime = 5;
    [SerializeField] private float minDistancePredict = 5;
    [SerializeField] private float maxDistancePredict = 100;
    [SerializeField] private float deviationSpeed = 2;
    [SerializeField] private float deviationAmount = 50;

    public GameObject impactParticles;

    private Vector3 standardPrediction;
    private Vector3 deviatedPrediction;

    public ulong parentClient;
    public GameObject target;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        rb.velocity = transform.forward * speed;
        if (target != null)
        {               
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

    private void OnCollisionEnter(Collision collision)
    {
        if (IsServer)
        {
            if (collision.gameObject.CompareTag("Spaceship"))
            {
                Game.instance.DealDamageToSpaceshipServerRpc(collision.gameObject.GetComponent<NetworkObject>().OwnerClientId, 30, parentClient);
                GameObject impactEffect = Instantiate(impactParticles, transform.position, Quaternion.Euler(Vector3.zero));
                impactEffect.GetComponentInChildren<ParticleSystem>().Play();
                Destroy(impactEffect, 0.5f);
                Destroy(gameObject);
            }
            if (collision.gameObject.CompareTag("Player"))
            {
                Game.instance.DealDamageToPlayerServerRpc(collision.gameObject.GetComponent<NetworkObject>().OwnerClientId, 30, parentClient);
                GameObject impactEffect = Instantiate(impactParticles, transform.position, Quaternion.Euler(Vector3.zero));
                impactEffect.GetComponentInChildren<ParticleSystem>().Play();
                Destroy(impactEffect, 0.5f);
                Destroy(gameObject);
            }
        }
    }
}
