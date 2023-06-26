using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Flower : MonoBehaviour
{
    public GameObject instructionPrefab;
    public Vector3 upDirection;
    private GameObject instructionUI;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            instructionUI = Instantiate(instructionPrefab, other.GetComponentInChildren<CanvasScaler>().transform);
            instructionUI.GetComponentInChildren<Text>().text = "Press " + KeybindManager.inputActions.Player.Pickup.GetBindingDisplayString() + " to pick up";
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            instructionUI.transform.position = other.GetComponentInChildren<Camera>().WorldToScreenPoint(transform.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(instructionUI);
        }
    }

    public void PickUp()
    {
        Destroy(instructionUI);
        Destroy(gameObject);
    }
}
