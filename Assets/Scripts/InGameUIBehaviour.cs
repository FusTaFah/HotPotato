using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameUIBehaviour : MonoBehaviour
{
    public void BackToMainMenu()
    {
        GameObject networkManager = GameObject.Find("NetworkManager"); //this is awful, but it's the only implementation i can think of given the stupid fucking DontDestroyOnLoad bullshit
        NetworkUtility netUtil = networkManager.GetComponent<NetworkUtility>();
        netUtil.Shutdown();
        Destroy(networkManager);
        UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScreen");
    }
}
