using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.UI;

public class ToolSwitching : NetworkBehaviour
{
    public GameObject[] fpsObjects;
    public GameObject[] bodyObjects;

    private void Start()
    {
        if (IsOwner)
        {
            UpdateSlots(0);
        }
    }

    public void SetSlotActive(InputAction.CallbackContext obj)
    {
        if (IsOwner)
        {            
            int slot = int.Parse(obj.action.name[^1].ToString());

            UpdateSlots(slot - 1);
            
        }
    }

    private void UpdateSlots(int activeObjectIndex)
    {
        for (int i = 0; i < fpsObjects.Length; i++)
        {
            if (i == activeObjectIndex)
            {
                fpsObjects[i].SetActive(true);
                bodyObjects[i].SetActive(true);
            }
            else
            {
                fpsObjects[i].SetActive(false);
                bodyObjects[i].SetActive(false);
            }
        }
        UpdateSlotsServerRpc(activeObjectIndex);
        Debug.Log(activeObjectIndex + 1);
    }

    [ServerRpc] private void UpdateSlotsServerRpc(int activeObjectIndex)
    {
        UpdateSlotsClientRpc(activeObjectIndex);
    }

    [ClientRpc] private void UpdateSlotsClientRpc(int activeObjectIndex)
    {
        if (!IsOwner)
        {
            for (int i = 0; i < fpsObjects.Length; i++)
            {
                if (i == activeObjectIndex)
                {
                    fpsObjects[i].SetActive(true);
                    bodyObjects[i].SetActive(true);
                }
                else
                {
                    fpsObjects[i].SetActive(false);
                    bodyObjects[i].SetActive(false);
                }
            }

        }
    }
}
