using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetTexture : MonoBehaviour
{
    public Material mipmap0;
    public Material mipmap1;

    private MeshRenderer meshRenderer;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }
    public void SwapMipmap()
    {
        if (meshRenderer.material = mipmap0)
        {
            meshRenderer.material = mipmap1;
        }
        else
        {
            meshRenderer.material = mipmap0;
        }
    }
}
