using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public int numObjects = 20;
    public GameObject prefab;
    public GameObject sphere;
    public int radius = 50;

    public GameObject impactEffect;
    public GameObject objectToPlace;

    void Start()
    {
        /*Vector3 center = sphere.transform.position;
        for (int i = 0; i < numObjects; i++)
        {
            Vector3 pos = RandomSphere(center, radius);
            Quaternion rot = Quaternion.FromToRotation(-Vector3.forward, center - pos);
            GameObject instantiatedObject = Instantiate(prefab, sphere.transform);
            instantiatedObject.transform.position = pos;
            instantiatedObject.transform.rotation = rot;

        }*/
    }

    Vector3 RandomSphere(Vector3 center, float radius)
    {
        float phi = Random.value * 360;
        float alpha = Random.value * 360;
        Vector3 pos;
        pos.x = center.x + radius * Mathf.Sin(phi * Mathf.Deg2Rad) * Mathf.Cos(alpha * Mathf.Deg2Rad);
        pos.y = center.y + radius * Mathf.Sin(alpha * Mathf.Deg2Rad);
        pos.z = center.z + radius * Mathf.Cos(phi * Mathf.Deg2Rad) * Mathf.Cos(alpha * Mathf.Deg2Rad);
        return pos;
    }

    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            PlaceObject();
        }*/
    }

    private void PlaceObject()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
            GameObject particle = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            particle.GetComponent<ParticleSystem>().Play();
            Destroy(particle, 2f);
            Vector3 center = hit.transform.position;
            SpawnObject(targetPoint, center);
        }
    }

    private void SpawnObject(Vector3 location, Vector3 center)
    {
        Quaternion rot = Quaternion.FromToRotation(-Vector3.up, center - location);
        Instantiate(prefab, location, rot);
    }
}