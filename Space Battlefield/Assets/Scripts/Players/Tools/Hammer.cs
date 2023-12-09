using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class Hammer : NetworkBehaviour
{
    private PlayerNetwork playerNetwork;

    private float range = 2;
    public LayerMask interactableObjects;
    public int miningPower;
    public int repairPower;

    public Animator handAnimator;
    public Animator animator;

    public AudioManager audioManager;

    private void Start()
    {
        GameObject[] playerRoots = GameObject.FindGameObjectsWithTag("Root");
        foreach(GameObject playerRoot in playerRoots)
        {
            if (playerRoot.GetComponent<NetworkObject>().OwnerClientId == OwnerClientId)
            {
                playerNetwork = playerRoot.GetComponent<PlayerNetwork>();
            }
        }
    }

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
            if (CheckAnimationFinished())
            {
                animator.Play("Mine");
                handAnimator.Play("Mine");

                StartCoroutine(ActionDelay());
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
    
    private IEnumerator ActionDelay()
    {
        yield return new WaitForSeconds(1f);

        Ray ray = new Ray(GetComponentInParent<Camera>().transform.position, GetComponentInParent<Camera>().transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range, interactableObjects))
        {
            //Mining
            if (hit.transform.GetComponent<Rock>())
            {
                hit.transform.GetComponent<Rock>().Mine(miningPower);
                MineServerRpc(ObjectDictionary.instance.GetIdOfObject(hit.transform.gameObject));
                Game.instance.GiveObjectToPlayerServerRpc(OwnerClientId, Item.Rock, 2);
                audioManager.Play("stone-mining");
            }               
        }       
    }

    [ServerRpc] private void MineServerRpc(int objectId)
    {
        MineClientRpc(objectId);
    }

    [ClientRpc] private void MineClientRpc(int objectId)
    {
        if (!IsOwner) 
        {
            GameObject rock = ObjectDictionary.instance.GetObjectById(objectId);            
            rock.GetComponent<Rock>().Mine(miningPower);
        }
    }
}
