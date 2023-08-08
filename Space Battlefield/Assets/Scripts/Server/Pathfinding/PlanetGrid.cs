using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetGrid : MonoBehaviour
{
    /*public float rayCastSphereScale = 50;
    public int points = 128;
    public LayerMask ground;
    public GameObject emptyPrefab;

    public List<Point> grid;
 
    void Start()
    {
        float scaling = rayCastSphereScale;
        Vector3[] pointArray = PointsOnSphere(points);
        List<GameObject> rayCastSpheres = new List<GameObject>();
        int i = 0;

        foreach (Vector3 value in pointArray)
        {
            rayCastSpheres.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
            rayCastSpheres[i].transform.parent = transform;
            rayCastSpheres[i].transform.position = value * scaling + transform.position;
            rayCastSpheres[i].name = i.ToString();
            Ray ray = new Ray(rayCastSpheres[i].transform.position, (transform.position - rayCastSpheres[i].transform.position).normalized);
            Debug.DrawRay(rayCastSpheres[i].transform.position, (transform.position - rayCastSpheres[i].transform.position).normalized);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 10000, ground))
            {
                if (hit.transform.gameObject.layer != 8)
                {
                    grid.Add(new Point());
                    
                }
            }
            i++;
        }
    }

    Vector3[] PointsOnSphere(int n)
    {
        List<Vector3> pointList = new List<Vector3>();
        float increment = Mathf.PI * (3 - Mathf.Sqrt(5));
        float offset = 2.0f / n;
        float x = 0;
        float y = 0;
        float z = 0;
        float r = 0;
        float phi = 0;

        for (int k = 0; k < n; k++)
        {
            y = k * offset - 1 + (offset / 2);
            r = Mathf.Sqrt(1 - y * y);
            phi = k * increment;
            x = Mathf.Cos(phi) * r;
            z = Mathf.Sin(phi) * r;

            pointList.Add(new Vector3(x, y, z));
        }
        Vector3[] pts = pointList.ToArray();
        return pts;
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawWireSphere(transform.position, rayCastSphereRadius);        
    }*/
}
