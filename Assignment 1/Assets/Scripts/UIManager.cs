using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class UIManager : MonoBehaviour
{
    public GameObject ConnectScreen;
    public GameObject Lobby;
    public GameObject JoinPlayer;
    public GameObject GameChat;
    public GameObject ConnectedPlayers;

    public Text LobbyLog;
    public Text JoinPlayerText;
    public Text PlayerList;
    public Text Chat;

    public InputField IpField;
    public InputField UsernameField;
    public InputField ChatField;

    public Button JoinButton;
    public Button AcceptPlayerButton;
    public Button RejectPlayerButton;
    public Button LeaveGameButton;

    private bool isConnected = false;
    private bool inLobby = false;
    private bool inGame = false;
    //private bool fieldFilled = false;

    // Start is called before the first frame update
    void Start()
    {
        ConnectScreen.SetActive(true);
        Lobby.SetActive(false);
        JoinPlayer.SetActive(false);
        GameChat.SetActive(false);
        ConnectedPlayers.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(inLobby)
        {
            LobbyScreen();
        }
        else if(inGame)
        {
            ChatScreen();
        }


        if (isConnected) { Debug.Log("Connection Successful"); }
    }

    public void Connect()
    {
        if (!isConnected && IpField.text != "" && UsernameField.text != "")
        {
            NetworkManager.ConnectToServer(IpField.text, UsernameField.text);
            /*
            if (NetworkManager.connected)
            {
                ConnectScreen.SetActive(false);
                Lobby.SetActive(true);
                ConnectedPlayers.SetActive(true);
                isConnected = true;
                inLobby = true;
            }
            */
        }
    }

    void LobbyScreen()
    {
        
    }

    void ChatScreen()
    {
        if (ChatField.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                string temp = ChatField.text;
                Debug.Log(temp);
                ChatField.text = "";
            }
        }
    }
}
