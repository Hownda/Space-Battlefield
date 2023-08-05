using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtmosphereGenerator : MonoBehaviour
{
    public Material atmosphereMaterial;
    public Transform sun;
    public Transform planet;
    public float planetRadius = 1f;
    public Vector3 waveLengths = new Vector3(700, 530, 440);
    public float scatteringStrength = 1f;
    public float densityFalloff = 2f;

    [Range(1, 10)]
    public float atmosphereScale = 1;

    public void Start()
    {
        SetScatterings();
    }

    void Update()
    {
        SetPositions();
    }

    private void OnValidate()
    {
        SetScatterings();
        SetPositions();
    }

    void SetScatterings()
    {
        float scatterR = Mathf.Pow(400 / waveLengths.x, 4) * scatteringStrength;
        float scatterG = Mathf.Pow(400 / waveLengths.y, 4) * scatteringStrength;
        float scatterB = Mathf.Pow(400 / waveLengths.z, 4) * scatteringStrength;
        Vector3 scatteringCoefficients = new Vector3(scatterR, scatterG, scatterB);
        atmosphereMaterial.SetVector("_ScatteringCoefficients", scatteringCoefficients);
        atmosphereMaterial.SetFloat("_DensityFalloff", densityFalloff);
    }

    void SetPositions()
    {
        Vector3 directionToLight = (sun.position - planet.position).normalized;
        atmosphereMaterial.SetVector("_DirToSun", directionToLight);
        atmosphereMaterial.SetVector("_PlanetCenter", planet.position);

        float atmosphereSize = planetRadius * (1 + atmosphereScale) - planetRadius;
        atmosphereMaterial.SetFloat("_PlanetRadius", planetRadius);
        atmosphereMaterial.SetFloat("_AtmosphereRadius", atmosphereSize);
    }
}