using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;

public class KeybindManager : MonoBehaviour
{
    public static MovementControls inputActions;

    public static event Action rebindComplete;
    public static event Action rebindCancelled;
    public static event Action<InputAction, int> rebindStarted;

    private void Awake()
    {
        if (inputActions == null)
        {
            inputActions = new MovementControls();
        }
    }

    public static void StartRebind(string actionName, int bindingIndex, Text statusText)
    {
        InputAction action = inputActions.asset.FindAction(actionName);
        if (action == null || action.bindings.Count <= bindingIndex)
        {
            Debug.Log("Couldn't find action or binding");
            return;
        }

        if (action.bindings[bindingIndex].isComposite)
        {
            var firstPartIndex = bindingIndex + 1;
            if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite)
            {
                DoRebind(action, firstPartIndex, statusText, true, action.bindings[firstPartIndex]);
            }
        }
        else
        {
            DoRebind(action, bindingIndex, statusText, false, action.bindings[bindingIndex]);
        }
    }

    private static void DoRebind(InputAction actionToRebind, int bindingIndex, Text statusText, bool allCompositeParts, InputBinding prevBinding)
    {
        if (actionToRebind == null || bindingIndex < 0)
        {
            return;
        }
        if (allCompositeParts)
        {
            statusText.text = $"Press 4 keys";
        }
        else
        {
            statusText.text = $"Press a key";
        }

        actionToRebind.Disable();

        var rebind = actionToRebind.PerformInteractiveRebinding(bindingIndex);

        rebind.OnComplete(operation =>
        {
            actionToRebind.Enable();
            operation.Dispose();

            var newBinding = actionToRebind.bindings[0];

            var bindings = actionToRebind.actionMap.bindings;
            foreach (var binding in bindings)
            {
                if (binding.action == newBinding.action)
                    continue;
                if (binding.effectivePath == newBinding.effectivePath)
                    actionToRebind.ChangeBinding(0).To(prevBinding);
                    rebindCancelled?.Invoke();
            }

            if (allCompositeParts)
            {
                var nextBindingIndex = bindingIndex + 1;
                if (nextBindingIndex < actionToRebind.bindings.Count && actionToRebind.bindings[nextBindingIndex].isPartOfComposite)
                {
                    DoRebind(actionToRebind, nextBindingIndex, statusText, allCompositeParts, actionToRebind.bindings[nextBindingIndex]);
                }
            }
            SaveBindingOverride(actionToRebind);
            rebindComplete?.Invoke();
        });

        rebind.OnCancel(operation =>
        {
            actionToRebind.Enable();
            operation.Dispose();

            rebindCancelled?.Invoke();
        });

        rebindStarted?.Invoke(actionToRebind, bindingIndex);
        rebind.Start();
    }

    public static string GetBindingName(string actionName, int bindingIndex)
    {
        if (inputActions == null)
        {
            inputActions = new MovementControls();
        }
        InputAction action = inputActions.asset.FindAction(actionName);
        return action.GetBindingDisplayString(bindingIndex);
    }

    private static void SaveBindingOverride(InputAction action)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            PlayerPrefs.SetString(action.actionMap + action.name + i, action.bindings[i].overridePath);
        }
    }

    public static void LoadBindingOverride(string actionName)
    {
        if (inputActions == null)
        {
            inputActions = new MovementControls();
        }
        if (inputActions.asset.FindAction(actionName) == null)
        {
            return;
        }
        InputAction action = inputActions.asset.FindAction(actionName);

        for (int i = 0; i < action.bindings.Count; i++)
        {
            if (!string.IsNullOrEmpty(PlayerPrefs.GetString(action.actionMap + action.name + i)))
            {
                action.ApplyBindingOverride(i, PlayerPrefs.GetString(action.actionMap + action.name + i));
            }
        }
    }

    public static void ResetBinding(string actionName, int bindingIndex)
    {
        InputAction action = inputActions.asset.FindAction(actionName);

        if (action == null || action.bindings.Count <= bindingIndex)
        {
            Debug.Log("Could not find action or binding");
            return;
        }

        if (action.bindings[bindingIndex].isComposite)
        {
            for (int i = bindingIndex; i < action.bindings.Count && action.bindings[i].isComposite; i++)
            {
                action.RemoveBindingOverride(i);
            }
        }
        else
        {
            action.RemoveBindingOverride(bindingIndex);
        }
        SaveBindingOverride(action);
    }
}
