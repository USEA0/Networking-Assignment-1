using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public enum GameState { 
    InStart,
    InLobby,
    InGame,
}

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

    //Game Screens
    public GameObject ConnectScreen;
    public GameObject Lobby;
    public GameObject JoinPlayer;
    public GameObject GameChat;
    public GameObject ConnectedPlayers;

    //start widgets
    public InputField IpField;
    public InputField UsernameField;
    public Button JoinButton;

    //lobby widgets
    public Text LobbyLog;

    //connected players
    public ButtonData[] allButtons;

    //join player
    public Text joinGameText;

    //chat
    public InputField chatText;

    public GameState state = GameState.InStart;

    // Start is called before the first frame update
    void Start()
    {
        ConnectScreen.SetActive(true);
        Lobby.SetActive(false);
        JoinPlayer.SetActive(false);
        GameChat.SetActive(false);
        ConnectedPlayers.SetActive(false);
        state = GameState.InStart;

        foreach (ButtonData button in allButtons) {
            button.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (state) {
            case GameState.InStart:
                if (!ConnectScreen.activeSelf)
                {
                    ConnectScreen.SetActive(true);
                    Lobby.SetActive(false);
                    GameChat.SetActive(false);
                    ConnectedPlayers.SetActive(false);
                }
                break;

            case GameState.InLobby:
                if (!Lobby.activeSelf)
                {
                    ConnectScreen.SetActive(false);
                    Lobby.SetActive(true);
                    GameChat.SetActive(false);
                    ConnectedPlayers.SetActive(true);
                }

                break;

            case GameState.InGame:
                if (!GameChat.activeSelf)
                {
                    ConnectScreen.SetActive(false);
                    Lobby.SetActive(false);
                    GameChat.SetActive(true);
                    ConnectedPlayers.SetActive(true);
                }
                break;

            default:
                break;
        }
    }

    //sets all connected players
    public void SetConnectedPlayers(List<PlayerProfile> players) {

        //reset buttons
        foreach (ButtonData button in allButtons)
        {
            button.gameObject.SetActive(false);
        }

        for (int counter = 0; counter < players.Count; counter++) {
            allButtons[counter].gameObject.SetActive(true);
            allButtons[counter].Setup(players[counter]);

            if (counter == NetworkManager.playerNumber) {
                allButtons[counter].gameObject.GetComponent<Button>().interactable = false;
            }
        }
    }

    //connects to server
    public void Connect()
    {
        if (!NetworkManager.connected && IpField.text != "" && UsernameField.text != "")
        {
            NetworkManager.ConnectToServer(IpField.text, UsernameField.text);
        }
    }

    //connection is confirmed
    public void ConfirmConnection()
    {
        state = GameState.InLobby;

        LobbyLog.text += UsernameField.text;
        LobbyLog.text += " has connected!\n";
    }

    public void SetupRequest(string userName) {
        JoinPlayer.SetActive(true);
        joinGameText.text = userName + " requests a game.";
    }

    public void GoToChat()
    {
        state = GameState.InGame;
    }

    public void Denied() {
        LobbyLog.text += "User Request for game was denied";
    }

    public void QuitGame() {
        NetworkManager.QuitGame();
        state = GameState.InLobby;
    }

    public void JoinGame() {
        NetworkManager.RespondToRequest(true);
        state = GameState.InGame;
        JoinPlayer.SetActive(false);
    }
    public void RejectGame() {
        NetworkManager.RespondToRequest(false);
        LobbyLog.text += "Rejected game";
        JoinPlayer.SetActive(false);
    }

    public void OnSendMessage() {
        NetworkManager.OnSendMessage(chatText.text);
        chatText.text = "";

    }

}
