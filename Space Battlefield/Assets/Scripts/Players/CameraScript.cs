using UnityEngine;
using Unity.Netcode;

public class CameraScript : NetworkBehaviour
{
    public Transform playerBody;
    public Camera spaceshipCamera;
    private Camera playerCamera;
    public GameObject weapon;
    public GameObject playerCanvas;

    private float xRotation = 0f;
    public float mouseSensitivity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        playerCamera = GetComponent<Camera>();

        if (!IsOwner)
        {
            playerCamera.enabled = false;
            playerCamera.GetComponent<AudioListener>().enabled = false;
            playerCanvas.SetActive(false);
        }
    }

    void Update()
    {
        if (IsOwner && PlayerData.instance.disableCameraMovement == false)
        {
            mouseSensitivity = PlayerData.instance.mouseSensitivity;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }

    public void DisableBodyParts()
    {
        if (!IsOwner)
        {
            // Makes other player visible
            transform.parent.gameObject.layer = 0;
            Renderer[] renderers = transform.parent.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.gameObject.layer = 0;
            }

            // Makes floating weapon invisible
            Renderer[] weaponRenderers = weapon.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in weaponRenderers)
            {
                renderer.gameObject.layer = 7;
            }
        }
    }
}
