using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.UI;

public class ToolSwitching : NetworkBehaviour
{
    private MovementControls gameActions;

    public GameObject[] hammers;
    public GameObject[] blasters;

    public Image toolbarHammerBackground;
    public Image toolbarBlasterBackground;
    public Text toolbarHammerText;
    public Text toolbarBlasterText;

    private void Awake()
    {
        gameActions = KeybindManager.inputActions;
        gameActions.Player.Hotbar1.started += SetBlasterActive;
        gameActions.Player.Hotbar2.started += SetHammerActive;
        gameActions.Player.Enable();
    }

    private void Start()
    {
        if (IsOwner)
        {
            toolbarHammerText.text = KeybindManager.inputActions.Player.Hotbar2.GetBindingDisplayString();
            toolbarBlasterText.text = KeybindManager.inputActions.Player.Hotbar1.GetBindingDisplayString();
        }               
    }

    public void SetBlasterActive(InputAction.CallbackContext obj)
    {
        if (IsOwner)
        {
            foreach (GameObject hammer in hammers)
            {
                hammer.SetActive(false);
                toolbarHammerBackground.color = new Color(127.5f, 0, 0, .3f);
            }
            GetComponentInChildren<Hammer>().enabled = false;
            foreach (GameObject blaster in blasters)
            {
                blaster.SetActive(true);
                toolbarBlasterBackground.color = new Color(0, 255, 0, .3f);
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
                toolbarHammerBackground.color = new Color(0, 255, 0, .3f);
            }
            GetComponentInChildren<Hammer>().enabled = true;
            foreach (GameObject blaster in blasters)
            {
                blaster.SetActive(false);
                toolbarBlasterBackground.color = new Color(127.5f, 0, 0, .3f);

            }
        }
    }
}
