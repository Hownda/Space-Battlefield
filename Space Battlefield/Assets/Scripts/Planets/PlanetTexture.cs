using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetTexture : MonoBehaviour
{
    public Material material0;
    public Material material1;

    private MeshRenderer meshRenderer;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }
    public void ChangeScaleLevel(int mipmapLevel)
    {
        if (mipmapLevel == 0)
        {
            meshRenderer.material = material0;
        }
        else
        {
            meshRenderer.material = material1;
        }
    }
}
