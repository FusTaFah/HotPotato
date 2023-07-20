using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class CameraBehaviour : MonoBehaviour
{
    bool found = false;
    Cinemachine.CinemachineVirtualCamera mainCam;
    // Start is called before the first frame update
    void Start()
    {
        mainCam = gameObject.GetComponent<Cinemachine.CinemachineVirtualCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!found)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            GameObject[] fakePlayers = GameObject.FindGameObjectsWithTag("PlayerLocal");
            foreach(GameObject plr in fakePlayers)
            {
                SetCameraTarget(plr);
                found = true;
                return;
            }
            foreach (GameObject player in players)
            {
                if (player.GetComponent<NetworkBehaviour>().IsLocalPlayer || SceneManager.GetActiveScene().name == "BetweenScene")
                {
                    SetCameraTarget(player);
                    found = true;
                }
            }
        }
    }

    public void SetCameraTarget(GameObject target)
    {
        mainCam.Follow = target.transform;
        mainCam.LookAt = target.transform;
    }
}
