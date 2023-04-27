using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class NetworkUtility : MonoBehaviour
{
    private NetworkManager netManager;
    private UnityTransport transport;
    [SerializeField]
    private GameObject playerPrefab;
    private float timeSinceJoinTimer;
    [SerializeField]
    private float TimeToWaitUntilSpawn;
    private bool joinedMainScene = false;
    private Queue<ulong> userIDQueue = new();

    private bool isMaster
    {
        get => netManager.IsHost || netManager.IsServer;
    }
    // Start is called before the first frame update
    void Start()
    {
        netManager = gameObject.GetComponent<NetworkManager>();
        transport = gameObject.GetComponent<UnityTransport>();
        netManager.OnClientConnectedCallback += OnJoin;
        netManager.OnClientDisconnectCallback += OnLeave;
        SceneManager.sceneLoaded += (scene, LoadSceneMode) => OnJoinScene(scene);

    }

    private void Update()
    {
        if (isMaster)
        {
            if (joinedMainScene)
            {
                timeSinceJoinTimer += Time.deltaTime;

                if (timeSinceJoinTimer >= TimeToWaitUntilSpawn)
                {
                    SpawnPlayers();
                }
            }
        }
    }

    private void OnJoin(ulong userID)
    {
        userIDQueue.Enqueue(userID);
        if(SceneManager.GetActiveScene().name != "SampleScene")
        {
            SceneManager.LoadScene("SampleScene");
        }
    }

    private void OnLeave(ulong userID)
    {

    }

    private void OnJoinScene(Scene joinedScene)
    {
        if(joinedScene.name == "SampleScene")
        {
            joinedMainScene = true;
        }
        else
        {
            joinedMainScene = false;
        }
        timeSinceJoinTimer = 0.0f;
    }

    private void SpawnPlayers()
    {
        while(userIDQueue.Count > 0)
        {
            GameObject spawnPlayer = GameObject.Instantiate(playerPrefab, new Vector3(Random.value * 50.0f, 1f, Random.value * 50.0f), Quaternion.identity);
            spawnPlayer.GetComponent<NetworkObject>().SpawnAsPlayerObject(userIDQueue.Dequeue(), true);
        }
    }

    public void StartHost()
    {
        netManager.StartHost();
    }

    public void StartClient(string ipAddress)
    {
        transport.SetConnectionData(ipAddress, 7777);
        netManager.StartClient();
    }
}
