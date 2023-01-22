using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    private Vector3 startPosition;
    private Vector3 destination;

    void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (Vector3.Distance(transform.position, destination) <= 1)
        {
            Destroy(gameObject);
        }
    }

    public void SetDestination(Vector3 target)
    {
        destination = target;
    }
}
