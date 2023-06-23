using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.UI;

public class ToolSwitching : NetworkBehaviour
{
    private MovementControls gameActions;

    private Weapon[] weapons;
    private Hammer[] hammers;

    public Image toolbarHammerBackground;
    public Image toolbarWeaponBackground;
    public Text toolbarHammerText;
    public Text toolbarWeaponText;

    private void Awake()
    {
        gameActions = KeybindManager.inputActions;
        gameActions.Player.Hotbar1.started += SetWeaponActive;
        gameActions.Player.Hotbar2.started += SetHammerActive;
        gameActions.Player.Enable();
    }

    private void Start()
    {
        if (IsOwner)
        {
            toolbarHammerText.text = KeybindManager.inputActions.Player.Hotbar2.GetBindingDisplayString();
            toolbarWeaponText.text = KeybindManager.inputActions.Player.Hotbar1.GetBindingDisplayString();
        }
        hammers = GetComponentsInChildren<Hammer>();
        weapons = GetComponentsInChildren<Weapon>();
        ActivateWeapon();
        SetWeaponActiveServerRpc();
    }

    public void SetWeaponActive(InputAction.CallbackContext obj)
    {
        if (IsOwner)
        {
            ActivateWeapon();
            //SetWeaponActiveServerRpc();
        }
    }

    private void ActivateWeapon()
    {
        foreach (Hammer hammer in hammers)
        {
            hammer.gameObject.SetActive(false);
            toolbarHammerBackground.color = new Color(0, 0, 0, 0);
        }

        foreach (Weapon weapon in weapons)
        {
            weapon.gameObject.SetActive(true);
            toolbarWeaponBackground.color = new Color(.3f, .3f, .3f, .4f);
        }
    }

    public void SetHammerActive(InputAction.CallbackContext obj)
    {
        if (IsOwner)
        {
            ActivateHammer();
            //SetHammerActiveServerRpc();
        }
    }

    private void ActivateHammer()
    {
        foreach (Hammer hammer in hammers)
        {
            hammer.gameObject.SetActive(true);
            toolbarHammerBackground.color = new Color(.3f, .3f, .3f, .4f);
        }

        foreach (Weapon weapon in weapons)
        {
            weapon.gameObject.SetActive(false);
            toolbarWeaponBackground.color = new Color(0, 0, 0, 0);
        }
    }

    [ServerRpc] private void SetWeaponActiveServerRpc()
    {
        SetWeaponActiveClientRpc();
    }

    [ClientRpc] private void SetWeaponActiveClientRpc()
    {
        if (!IsOwner)
        {
            ActivateWeapon();
        }
    }

    [ServerRpc] private void SetHammerActiveServerRpc()
    {
        SetHammerActiveClientRpc();
    }

    [ClientRpc] private void SetHammerActiveClientRpc()
    {
        if (!IsOwner)
        {
            ActivateHammer();
        }
    }
}
