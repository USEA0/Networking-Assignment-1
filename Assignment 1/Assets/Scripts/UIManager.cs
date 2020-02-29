using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class UIManager : MonoBehaviour
{
    #region SingletonCode
    private static UIManager _instance;
    public static UIManager Instance { get { return _instance; } }
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    //single pattern ends here
    #endregion
    public GameObject ConnectScreen;
    public GameObject Lobby;
    public GameObject JoinPlayer;
    public GameObject GameChat;
    public GameObject[] ConnectedPlayers;
    public GameObject[] JoinPlayerButton;

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

    public 

    // Start is called before the first frame update
    void Start()
    {
        ConnectScreen.SetActive(true);
        Lobby.SetActive(false);
        JoinPlayer.SetActive(false);
        GameChat.SetActive(false);
        foreach (GameObject Connected in ConnectedPlayers)
        {
            Connected.SetActive(false);
        }
        foreach(GameObject ButtonJoin in JoinPlayerButton)
        {
            ButtonJoin.SetActive(false);
        }
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


    }

    public void Connect()
    {
        if (!isConnected && IpField.text != "" && UsernameField.text != "")
        {
            NetworkManager.ConnectToServer(IpField.text, UsernameField.text);

        }
    }

    public void connectTest()
    {

        ConnectScreen.SetActive(false);
        Lobby.SetActive(true);
        ConnectedPlayers.SetActive(true);
        isConnected = true;
        inLobby = true;
        LobbyLog.text += UsernameField.text;
        LobbyLog.text += " has connected!\n";
    }

    public void players(string playerData)
    {
        PlayerList.text = playerData;
    }

    public void Join(int playerIndex)
    {

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
