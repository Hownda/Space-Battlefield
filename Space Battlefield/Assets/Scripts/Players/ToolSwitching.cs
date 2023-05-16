using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class ToolSwitching : NetworkBehaviour
{
    private MovementControls gameActions;

    public GameObject[] hammers;
    public GameObject[] blasters;

    private void Awake()
    {
        gameActions = KeybindManager.inputActions;
        gameActions.Player.Hotbar1.started += SetBlasterActive;
        gameActions.Player.Hotbar2.started += SetHammerActive;
        gameActions.Player.Enable();
    }

    public void SetBlasterActive(InputAction.CallbackContext obj)
    {
        if (IsOwner)
        {
            foreach (GameObject hammer in hammers)
            {
                hammer.SetActive(false);
            }
            foreach (GameObject blaster in blasters)
            {
                blaster.SetActive(true);
                
            }
        }
    }

    public void SetHammerActive(InputAction.CallbackContext obj)
    {
        if (IsOwner)
        {
            foreach (GameObject hammer in hammers)
            {
                hammer.SetActive(true);
            }
            foreach (GameObject blaster in blasters)
            {
                blaster.SetActive(false);

            }
        }
    }
}
