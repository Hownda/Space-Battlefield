using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitInfo
{
    public Vector3 point;
    public float radius;
    public Color color;
    public float force;
    public float duration;
    public float timeStamp;

    public HitInfo(Vector3 point, float radius, Color color, float force, float duration, float timeStamp)
    {
        this.point = point;
        this.radius = radius;
        this.color = color;
        this.force = force;
        this.duration = duration;
        this.timeStamp = timeStamp;
    }
}

public class Shield : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private Material material;
    private List<HitInfo> hits = new();

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        material = meshRenderer.material;
    }

    public void Hit(Vector3 point, float radius, Color color, float force, float duration)
    {
        var hitInfo = new HitInfo(point, radius, color, force, duration, Time.time);
        if (hits.Count >= 4)
        {
            hits.RemoveAt(0);
        }
        hits.Add(hitInfo);
        Debug.Log("Added hit");
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.tag);
        Hit(collision.contacts[0].point, 1f, Color.yellow, (collision.contacts[0].normal.magnitude / 10f), 1f);
    }

    private void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            Vector3 position = Vector3.zero;
            Color color = Color.black;
            Vector4 data = Vector4.zero;
            if (hits.Count > i)
            {
                float f = (Time.time - hits[i].timeStamp) / hits[i].duration;
                if (f < 1f)
                {
                    position = hits[i].point;
                    color = hits[i].color;
                    data = new Vector4(hits[i].radius, f, hits[i].force, 0);
                }
                else
                {
                    hits.RemoveAt(i);
                }
            }
            material.SetVector("HitPosition" + (i + 1), position);
            material.SetColor("HitColor" + (i + 1), color);
            material.SetVector("HitData" + (i + 1), data);
        }
    }
}
