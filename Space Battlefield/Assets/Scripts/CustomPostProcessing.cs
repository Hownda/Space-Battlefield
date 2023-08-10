using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class CustomPostProcessing : MonoBehaviour
{
    public Material postProcessingMaterial;
    public Vector3 waveLengths = new Vector3(700, 530, 440);
    public float scatteringStrength = 1;

    private void OnValidate()
    {
        float scatterR = Mathf.Pow(400 / waveLengths.x, 4) * scatteringStrength;
        float scatterG = Mathf.Pow(400 / waveLengths.y, 4) * scatteringStrength;
        float scatterB = Mathf.Pow(400 / waveLengths.z, 4) * scatteringStrength;
        Vector3 scatteringCoefficients = new Vector3(scatterR, scatterG, scatterB);
        postProcessingMaterial.SetVector("scatteringCoefficients", scatteringCoefficients);
    }
}
