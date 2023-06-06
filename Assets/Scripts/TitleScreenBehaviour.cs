using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class TitleScreenBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private GameObject buttons;
    [SerializeField]
    private GameObject Join;
    [SerializeField]
    private GameObject Host;
    [SerializeField]
    private GameObject Server;
    [SerializeField]
    private GameObject Joining;
    [SerializeField]
    private GameObject BackButton;
    [SerializeField]
    private TMPro.TMP_InputField IPTextField;
    [SerializeField]
    private NetworkUtility utility;
    private NetworkUtility Utility
    {
        get
        {
            if(utility == null)
            {
                utility = GameObject.Find("NetworkManager").GetComponent<NetworkUtility>();
            }
            return utility;
        }
    }
    [SerializeField]
    private TMPro.TMP_Text ipDisplay;

    private TitleState state = TitleState.TITLE;

    public enum TitleState
    {
        TITLE,
        JOIN,
        HOST,
        SERVER,
        JOINING
    }

    private void Start()
    {
        if (GameObject.FindGameObjectWithTag("Finish") == null)
        {
            Instantiate(Utility);
        }
        SetMode(TitleState.TITLE);
        Utility.TransportEventDelegate = TransportDelegate;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnJoinButtonPressed()
    {
        SetMode(TitleState.JOIN);
    }

    public void OnHostButtonPressed()
    {
        SetMode(TitleState.HOST);
    }

    public void OnServerButtonPressed()
    {
        SetMode(TitleState.SERVER);
    }

    public void OnBackButtonPressed()
    {
        SetMode(TitleState.TITLE);
    }
    
    public void OnConnectButtonPressed()
    {
        SetMode(TitleState.JOINING);
    }

    private void SetMode(TitleState st)
    {
        state = st;

        switch (state)
        {
            case TitleState.TITLE:
                ToggleChildren(buttons, true);
                Join.transform.GetChild(0).gameObject.SetActive(false);
                Host.transform.GetChild(0).gameObject.SetActive(false);
                Server.transform.GetChild(0).gameObject.SetActive(false);
                BackButton.transform.GetChild(0).gameObject.SetActive(false);
                Joining.transform.GetChild(0).gameObject.SetActive(false);
                break;
            case TitleState.JOIN:
                ToggleChildren(buttons, false);
                Join.transform.GetChild(0).gameObject.SetActive(true);
                Host.transform.GetChild(0).gameObject.SetActive(false);
                Server.transform.GetChild(0).gameObject.SetActive(false);
                BackButton.transform.GetChild(0).gameObject.SetActive(true);
                Joining.transform.GetChild(0).gameObject.SetActive(false);
                break;
            case TitleState.HOST:
                ToggleChildren(buttons, false);
                Join.transform.GetChild(0).gameObject.SetActive(false);
                Host.transform.GetChild(0).gameObject.SetActive(true);
                Server.transform.GetChild(0).gameObject.SetActive(false);
                BackButton.transform.GetChild(0).gameObject.SetActive(true);
                Joining.transform.GetChild(0).gameObject.SetActive(false);
                break;
            case TitleState.SERVER:
                ToggleChildren(buttons, false);
                Join.transform.GetChild(0).gameObject.SetActive(true);
                Host.transform.GetChild(0).gameObject.SetActive(false);
                Server.transform.GetChild(0).gameObject.SetActive(false);
                BackButton.transform.GetChild(0).gameObject.SetActive(true);
                Joining.transform.GetChild(0).gameObject.SetActive(false);
                break;
            case TitleState.JOINING:
                ToggleChildren(buttons, false);
                Join.transform.GetChild(0).gameObject.SetActive(false);
                Host.transform.GetChild(0).gameObject.SetActive(false);
                Server.transform.GetChild(0).gameObject.SetActive(false);
                BackButton.transform.GetChild(0).gameObject.SetActive(true);
                Joining.transform.GetChild(0).gameObject.SetActive(true);
                ipDisplay.text = "Connecting to " + Utility.JoinConnectIPAddress + " on port " + Utility.JoinConnectPort;
                break;
        }
    }

    private void ToggleChildren(GameObject g_parent, bool toggle)
    {
        int childCount = g_parent.transform.childCount;
        for(int i = 0; i < childCount; i++)
        {
            g_parent.transform.GetChild(i).gameObject.SetActive(toggle);
        }
    }

    public void Connect()
    {
        Utility.StartClient(IPTextField.text);
    }

    public void ConnectHardCode()
    {
        utility.StartClient("192.168.0.10");
    }

    private void TransportDelegate(NetworkEvent eventType, ulong clientId, System.ArraySegment<byte> payload, float receiveTime)
    {
        switch (eventType)
        {
            case NetworkEvent.TransportFailure:
            case NetworkEvent.Disconnect:
                char[] pld = new char[payload.Count];
                for (int i = 0; i < pld.Length; i++)
                {
                    pld[i] = (char)payload[i];
                }
                string payld = new string(pld);
                ipDisplay.text =  $"{receiveTime}: could not connect. \n" + payld;
                break;
        }
    }

    public void CancelConnection()
    {
        if (Utility.IsClientStartedConnection)
        {
            Utility.CancelConnection();
        }
    }

    public void StartHost()
    {
        Utility.StartHost();
    }
}
