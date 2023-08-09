using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class CustomPostProcessing : MonoBehaviour
{
    public Material postProcessingMaterial;
    public int atmosphereRadius = 50;
    public GameObject planet;

    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //postProcessingMaterial.SetVector("planetCenter", planet.transform.position);
        //postProcessingMaterial.SetFloat("atmosphereRadius", atmosphereRadius);
        Graphics.Blit(source, destination, postProcessingMaterial);
    }
}
