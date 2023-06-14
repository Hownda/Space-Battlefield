using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class Hammer : NetworkBehaviour
{
    public float range;
    public LayerMask naturalResource;
    public float miningPower;

    public Animator handAnimator;
    public Animator animator;

    public GameObject rockItem;
    public Transform rockCollectionUI;
    public Text rockCounter;
    private int rockCount;

    public AudioManager audioManager;

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            MiningInput();
        }
    }

    private void MiningInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Ray ray = new Ray(GetComponentInParent<Camera>().transform.position, GetComponentInParent<Camera>().transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, range, naturalResource))
            {
                if (hit.transform.GetComponent<Resource>() && CheckAnimationFinished())
                {
                    animator.Play("Mine");
                    handAnimator.Play("Mine");
                    StartCoroutine(CollectionDelay(hit.transform));
                }
            }
        }
    }

    private bool CheckAnimationFinished()
    {
        if (!animator.GetAnimatorTransitionInfo(0).IsName("Mine"))
        {
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Mine"))
            {
                if (!handAnimator.GetCurrentAnimatorStateInfo(0).IsName("Mine"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
    
    private IEnumerator CollectionDelay(Transform rock)
    {
        yield return new WaitForSeconds(1f);
        rock.GetComponent<Resource>().Mine(miningPower);

        rockCount += 1;
        rockCounter.text = rockCount.ToString();
        GameObject rockAdditionUI = Instantiate(rockItem, rockCollectionUI);

        audioManager.Play("stone-mining");

        Destroy(rockAdditionUI, 2f);
    }
}
