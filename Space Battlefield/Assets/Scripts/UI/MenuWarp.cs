using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class MenuWarp : MonoBehaviour
{
    public VisualEffect warpSpeedVFX;
    public float rate = 0.02f;
    public float speed = 3;
    private bool warpActive;

    private Rigidbody spaceshipRb;
    private AudioSource engine;

    public float desiredVolume = 0.5f;

    public float desiredPitch = 1;

    public float finalPitch = 2;
    public float finalVolume = 1;

    public float interpolationTime = 10;

    // Start is called before the first frame update
    void Start()
    {
        warpActive = true;
        warpSpeedVFX.SetFloat("WarpAmount", 0);
        StartCoroutine(ActivateParticles());
        spaceshipRb = GameObject.FindGameObjectWithTag("Spaceship").GetComponent<Rigidbody>();
        engine = spaceshipRb.GetComponent<AudioSource>();  
    }

    private void FixedUpdate()
    {
        spaceshipRb.AddForce(spaceshipRb.transform.forward * speed * Time.fixedDeltaTime, ForceMode.Acceleration);
        spaceshipRb.maxLinearVelocity = 10;

        engine.pitch = Mathf.Lerp(engine.pitch, desiredPitch, .7f);
        engine.volume = Mathf.Lerp(engine.volume, desiredVolume, .7f);
    }

    private IEnumerator ActivateParticles()
    {
        if (warpActive)
        {
            warpSpeedVFX.Play();

            float amount = warpSpeedVFX.GetFloat("WarpAmount");
            while (amount < 1)
            {
                amount += rate;
                warpSpeedVFX.SetFloat("WarpAmount", amount);

                if (desiredVolume < finalVolume || desiredPitch < finalPitch)
                desiredPitch += rate;
                desiredVolume += rate / 3;                
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            warpSpeedVFX.Stop();
        }
    }

}
