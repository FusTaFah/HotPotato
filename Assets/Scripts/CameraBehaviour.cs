using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class CameraBehaviour : MonoBehaviour
{
    bool found = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!found)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                if (player.GetComponent<NetworkBehaviour>().IsLocalPlayer || SceneManager.GetActiveScene().name == "BetweenScene")
                {
                    Cinemachine.CinemachineVirtualCamera mainCam = gameObject.GetComponent<Cinemachine.CinemachineVirtualCamera>();
                    mainCam.Follow = player.transform;
                    mainCam.LookAt = player.transform;
                }
            }
        }
    }
}
