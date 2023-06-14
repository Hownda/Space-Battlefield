using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetTexture : MonoBehaviour
{
    public GameObject farGraphics;
    public GameObject closeGraphics;

    public void ChangeScaleLevel(int mipmapLevel)
    {
        if (mipmapLevel == 0)
        {
            farGraphics.SetActive(false);
            closeGraphics.SetActive(true);
        }
        else
        {
            closeGraphics.SetActive(false);
            farGraphics.SetActive(true);
        }
    }
}
