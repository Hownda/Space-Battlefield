using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public int numObjects = 20;
    public GameObject prefab;
    public GameObject sphere;
    private int radius = 95;

    void Start()
    {
        Vector3 center = sphere.transform.position;
        for (int i = 0; i < numObjects; i++)
        {
            Vector3 pos = RandomSphere(center, radius);
            Quaternion rot = Quaternion.FromToRotation(Vector3.forward, center - pos);
            Instantiate(prefab, pos, rot);
        }
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
}
