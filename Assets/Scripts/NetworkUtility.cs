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
    private UnityTransport Transport
    {
        get
        {
            if(transport == null)
            {
                transport = gameObject.GetComponent<UnityTransport>();
            }
            return transport;
        }
    }
    private NetworkManager NetManager
    {
        get
        {
            if(netManager == null)
            {
                netManager = gameObject.GetComponent<NetworkManager>();
            }
            return netManager;
        }
    }

    [SerializeField]
    private GameObject playerPrefab;
    private float timeSinceJoinTimer;
    [SerializeField]
    private float TimeToWaitUntilSpawn;
    private bool joinedMainScene = false;
    private Queue<ulong> userIDQueue = new();

    public string JoinConnectIPAddress => Transport.ConnectionData.Address;

    public int JoinConnectPort => Transport.ConnectionData.Port;

    private bool startedConnection = false;
    public bool IsClientStartedConnection => startedConnection;

    private bool isMaster
    {
        get => NetManager.IsHost || NetManager.IsServer;
    }

    public NetworkTransport.TransportEventDelegate TransportEventDelegate
    {
        set 
        {
            Transport.OnTransportEvent += value; 
        }
    }

    private static NetworkUtility playerInstance;

    void Awake()
    {
        if(playerInstance == null)
        {
            playerInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        NetManager.SetSingleton();
        NetManager.OnClientConnectedCallback += OnJoin;
        NetManager.OnClientDisconnectCallback += OnLeave;
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

    public void Shutdown()
    {
        NetManager.Shutdown();
        startedConnection = false;
    }

    public void CancelConnection()
    {
        NetManager.Shutdown();
        startedConnection = false;
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
        NetManager.StartHost();
    }

    public void StartClient(string ipAddress)
    {
        Transport.SetConnectionData(ipAddress, 7777, "0.0.0.0");
        NetManager.StartClient();
        startedConnection = true;
    }
}
