using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
    private GameObject BackButton;
    [SerializeField]
    private TMPro.TMP_InputField IPTextField;
    [SerializeField]
    private NetworkUtility utility;

    private TitleState state = TitleState.TITLE;

    public enum TitleState
    {
        TITLE,
        JOIN,
        HOST,
        SERVER
    }

    private void Start()
    {
        SetMode(TitleState.TITLE);
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
                break;
            case TitleState.JOIN:
                ToggleChildren(buttons, false);
                Join.transform.GetChild(0).gameObject.SetActive(true);
                Host.transform.GetChild(0).gameObject.SetActive(false);
                Server.transform.GetChild(0).gameObject.SetActive(false);
                BackButton.transform.GetChild(0).gameObject.SetActive(true);
                break;
            case TitleState.HOST:
                ToggleChildren(buttons, false);
                Join.transform.GetChild(0).gameObject.SetActive(false);
                Host.transform.GetChild(0).gameObject.SetActive(true);
                Server.transform.GetChild(0).gameObject.SetActive(false);
                BackButton.transform.GetChild(0).gameObject.SetActive(true);
                break;
            case TitleState.SERVER:
                ToggleChildren(buttons, false);
                Join.transform.GetChild(0).gameObject.SetActive(true);
                Host.transform.GetChild(0).gameObject.SetActive(false);
                Server.transform.GetChild(0).gameObject.SetActive(false);
                BackButton.transform.GetChild(0).gameObject.SetActive(true);
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
        utility.StartClient(IPTextField.text);
    }
}
