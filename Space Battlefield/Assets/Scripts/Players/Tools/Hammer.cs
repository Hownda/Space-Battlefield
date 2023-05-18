using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Hammer : NetworkBehaviour
{
    public float range;
    public LayerMask naturalResource;
    public float miningPower;

    public Animator handAnimator;
    public Animator animator;

    private float miningCooldown = 2f;
    private float lastSwing;

    private void Start()
    {
        lastSwing = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && IsOwner)
        {
            Ray ray = new Ray(GetComponentInParent<Camera>().transform.position, GetComponentInParent<Camera>().transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, range, naturalResource))
            {
                if (hit.transform.GetComponent<Resource>() && Time.time - lastSwing > miningCooldown)
                {
                    Debug.Log("Mine");
                    animator.SetBool("Mine", true);
                    handAnimator.SetBool("Mine", true);
                    hit.transform.GetComponent<Resource>().Mine(miningPower);
                    lastSwing = Time.time;
                }
            }
        }
        
    }
}
