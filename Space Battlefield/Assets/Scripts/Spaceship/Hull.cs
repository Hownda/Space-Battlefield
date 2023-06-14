using UnityEngine.UI;
using Unity.Netcode;

public class Hull : NetworkBehaviour
{
    public NetworkVariable<int> integrity = new NetworkVariable<int>(100, writePerm: NetworkVariableWritePermission.Server);
    public Slider integritySlider;

    // Start is called before the first frame update
    void Start()
    {
       if (IsOwner)
       {
            integritySlider.value = integrity.Value;
       }
       else
        {
            integritySlider.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        integritySlider.value = integrity.Value;
    }

    public void TakeDamage(int damage)
    {
        integrity.Value -= damage;
        if (integrity.Value - damage <= 0)
        {
            SelfDestruct();
        }
    }

    private void SelfDestruct()
    {
        GetComponent<SpaceshipMovement>().Exit();
        Destroy(gameObject);
    }
}
