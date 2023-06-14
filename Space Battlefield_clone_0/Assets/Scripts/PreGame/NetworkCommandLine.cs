using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Photon.Pun;

public class NetworkCommandLine : MonoBehaviour
{
    private NetworkManager networkManager;
    public GameObject NetworkButtons;

    private void Start()
    {
        networkManager = GetComponentInParent<NetworkManager>();

        //if (Application.isEditor)
        //{
            NetworkButtons.SetActive(true);
        //}
        /*else
        {
            var args = GetCommandLineArgs();

            if (args.TryGetValue("-mode", out string mode))
            {
                if (mode == "server")
                {
                    networkManager.StartServer();
                    Debug.Log("Starting Server...");
                }
                if (mode == "client")
                {
                    networkManager.StartClient();
                    Debug.Log("Starting Client...");
                }
            }
            else
            {
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(
                (string)PhotonNetwork.CurrentRoom.CustomProperties["ip"],
                (ushort)System.Convert.ToInt32((string)PhotonNetwork.CurrentRoom.CustomProperties["port"]),
                "0.0.0.0"
                );
                NetworkManager.Singleton.StartClient();
            }
        }*/
    }
    /*private Dictionary<string, string> GetCommandLineArgs()
    {
        Dictionary<string, string> argsDictionary = new Dictionary<string, string>();
        var args = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i].ToLower();
            if (arg.StartsWith("-"))
            {
                var value = i < args.Length - 1 ? args[1 + i].ToLower() : null;
                value = (value?.StartsWith("-") ?? false) ? null : value;

                argsDictionary.Add(arg, value);
            }
        }
        return argsDictionary;
    }*/
}
