using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Text;
using System;
using System.Collections.Generic;


public enum PacketType
{
    //initialization connection
    INIT_CONNECTION,
    //single string
    MESSAGE,
    //requests 
    REQUEST_GAME,
    //request responses
    REQUEST_RESPONSE,
    //quit game
    GAME_QUIT,
    //request data
    SESSION_DATA,
    LOBBY_DATA,

    //error
    ERROR_PACKET,
}

public class PlayerProfile {
    public string username = "";
    public int id = -1;
    public bool inGame = false;
}

public class NetworkManager : MonoBehaviour
{

    #region Netcode

    //net code
    [DllImport("CNET.dll")]
    static extern IntPtr CreateClient();                            //Creates a client
    [DllImport("CNET.dll")]
    static extern void DeleteClient(IntPtr client);                 //Destroys a client
    [DllImport("CNET.dll")]
    static extern void Connect(string str, string username, IntPtr client);          //Connects to c++ Server
    [DllImport("CNET.dll")]
    static extern void SendData(int type, string str, bool useTCP, IntPtr client);          //Sends Message to all other clients    
    [DllImport("CNET.dll")]
    static extern void StartUpdating(IntPtr client);                //Starts updating
    [DllImport("CNET.dll")]
    static extern int GetPlayerNumber(IntPtr client);
    [DllImport("CNET.dll")]
    static extern void SetupMessage(Action<int, int, string> action); //recieve packets from server

    [DllImport("CNET.dll")]
    static extern void SetupOnConnect(Action action); //recieve packets from server

    //pointer to the client dll
    private static IntPtr Client;

    //ip of the server
    public string ip;

    //local player number
    private static int playerNumber = -1;

    //connected status
    public static bool connected = false;


    #endregion

    public static bool connectFlag = false;
    //if the player is in a game(chat)
    public static bool inGame = false;

    //check if request is active
    public static bool requestActive = false;

    //holds the last active requester index
    public static int requesterIndex = -1;

    //holds username of requester / requestee
    public static string requestUsername = "";

    //flags for request and response events
    public static bool requestEvent = false;
    public static bool responseEvent = false;
    public static bool gameEntryEvent = false;

    //queue for storing messages
    public static Queue<string> MessageQueue = new Queue<string>();

    //list for storing lobby information
    public static List<PlayerProfile> ActiveLobby = new List<PlayerProfile>();
    public static bool lobbyUpdated = false;

    //list for storing session information
    public static List<PlayerProfile> ActiveSession = new List<PlayerProfile>();
    public static bool sessionUpdated = false;

    //tick timestep
    int fixedTimeStep = 0;
    //tick delay
    int tickDelay = 50;


    // Start is called before the first frame update
    void Awake()
    {
        Client = CreateClient();
        SetupMessage(PacketRecieved);
        SetupOnConnect(OnConnect);
    }

    //TODO call this to connect to server 
    static public void ConnectToServer(string ip, string username)
    {
        Connect(ip, username, Client);
        StartUpdating(Client);
    }

    //this will be called if the connection is successful
    static void OnConnect()
    {
        connected = true;
        connectFlag = true;
        playerNumber = GetPlayerNumber(Client);
    
        Debug.Log("Connected");
    }


    //update loop
    private void FixedUpdate()
    {
        #region Fixed Tick
        ++fixedTimeStep;

        if (fixedTimeStep >= tickDelay)
        {
            TickUpdate();
            fixedTimeStep = 0;
        }
        #endregion

        //process incomming packet updates here
        lock (MessageQueue) {
            //TODO:process all messages


        }

        if (lobbyUpdated)
        {
            lock (ActiveLobby)
            {
                //TODO:update current lobby
                StringBuilder playerData = new StringBuilder();

                foreach(PlayerProfile player in ActiveLobby)
                {
                    playerData.Append(player.username);
                    if(player.inGame)
                    {
                        playerData.Append("Busy – Playing a game");
                    }
                    else
                    {
                        playerData.Append("Avaliable");
                    }
                    playerData.Append("\n");

                }

                UIManager.Instance.players(playerData.ToString());

            }
            lobbyUpdated = false;
        }

        if (sessionUpdated)
        {
            lock (ActiveSession)
            {
                //TODO:update current active session




            }
            sessionUpdated = false;
        }


        //listen to events
        if (responseEvent) { OnRequestResponse(inGame); }
        if (requestEvent) { OnGameRequest(requesterIndex, requestUsername); }
        if (gameEntryEvent) { OnGameEntry(); }

        if (connectFlag)
        {
            UIManager.Instance.connectTest();
            connectFlag = false;
        }
    }


    //call for updates here
    void TickUpdate()
    {

        //request data from server
        if (inGame) { RequestLobbyData(); } else { RequestSessionData(); }
        
    }


    //called on data recieve action, then process
    static void PacketRecieved(int type, int sender, string data)
    {
        data.TrimEnd();

        //parse the data
        string[] parsedData = data.Split(',');

        switch ((PacketType)type) {
            //this recieves a message from another user
            case PacketType.MESSAGE:
                if (parsedData.Length == 2)
                {
                    StringBuilder newString = new StringBuilder();

                    newString.Append(parsedData[1]);
                    newString.Append(": ");
                    newString.Append(parsedData[0]);

                    //send to queue
                    MessageQueue.Enqueue(newString.ToString());
                }
                else {
                    Debug.LogWarning("Packet: MESSAGE Length is invalid");
                }
                break;

             //this recieves a game request from another user
            case PacketType.REQUEST_GAME:
                if (parsedData.Length == 1)
                {
                    requestActive = true;
                    requesterIndex = sender;
                    requestUsername = parsedData[0];
                    requestEvent = true;
                }
                else {
                    Debug.LogWarning("Packet: REQUEST_GAME Length is invalid");
                }
                break;

            //this recieves a response to a sent game request
            case PacketType.REQUEST_RESPONSE:
                if (parsedData.Length == 2) {
                    requestUsername = parsedData[1];

                    if (parsedData[0] == "1") {
                        inGame = true;
                        gameEntryEvent = true;
                    }
                    else {
                        inGame = false;
                    }
                    responseEvent = true;
                }
                else
                {
                    Debug.LogWarning("Packet: REQUEST_RESPONSE Length is invalid");
                }
                break;

            //this recieves all the server data from the lobby
            case PacketType.LOBBY_DATA:
                lobbyUpdated = true;

                List<PlayerProfile> newLobby = new List<PlayerProfile>();

                //setup lobby data updates
                for (int counter = 0; counter < parsedData.Length / 3; counter++) {
                    PlayerProfile tempProfile = new PlayerProfile();

                    //setup profile
                    tempProfile.id = int.Parse(parsedData[0 + counter]);
                    tempProfile.username = parsedData[1 + counter];

                    if (parsedData[2 + counter] == "1")
                    {
                        tempProfile.inGame = true;
                    }
                    else
                    {
                        tempProfile.inGame = false;
                    };

                    //add to lobby
                    newLobby.Add(tempProfile);
                }

                //replace old lobby with new
                ActiveLobby = newLobby;

                break;

            //this recieves all the session data from the lobby
            case PacketType.SESSION_DATA:
                sessionUpdated = true;

                List<PlayerProfile> newSession = new List<PlayerProfile>();

                //setup lobby data updates
                for (int counter = 0; counter < parsedData.Length / 3; counter++)
                {
                    PlayerProfile tempProfile = new PlayerProfile();

                    //setup profile
                    tempProfile.id = int.Parse(parsedData[0 + counter]);
                    tempProfile.username = parsedData[1 + counter];
                    tempProfile.inGame = bool.Parse(parsedData[2 + counter]);

                    //add to lobby
                    newSession.Add(tempProfile);
                }

                //replace old lobby with new
                ActiveSession = newSession;

                break;
            default:
                Debug.Log("Unexpected Datatype Recieved");

                break;
        }
    }

    //send single string message to all others
    public static void OnSendMessage(string message)
    {
        StringBuilder finalMessage = new StringBuilder();
        finalMessage.Append(message);
        SendData((int)PacketType.MESSAGE, finalMessage.ToString(), true, Client);
    }

    //call c++ cleanup
    private void OnDestroy()
    {
        //clean up client
        DeleteClient(Client);
    }

    public static void OnGameRequest(int requesterIndex, string requesterUsername) {
        requestEvent = false;
        //TODO: Process game request event

    }
    public static void OnRequestResponse(bool response)
    {
        responseEvent = false;
        //TODO: Process request response event

    }
    public static void OnGameEntry() {
        gameEntryEvent = false;
        //TODO: Process game entry

    }


    //TODO: call this to request a game from player of index
    public static void RequestGame(int index)
    {
        SendData((int)PacketType.REQUEST_GAME, index.ToString() , true, Client);
    }

    //TODO: call this to respond to a game request
    public static void RespondToRequest(bool acceptance)
    {
        if (!inGame)
        {
            requestActive = false;

            //join the game on local
            if (acceptance)
            {
                inGame = true;
                gameEntryEvent = true;
            }

            SendData((int)PacketType.REQUEST_RESPONSE, requesterIndex.ToString() + "," + acceptance.ToString(), true, Client);
        }
        else
        {
            Debug.LogWarning("Already in a game");
        }


    }

    //TODO: call this to quit the game
    public static void QuitGame()
    {
        if (inGame)
        {
            SendData((int)PacketType.GAME_QUIT, "", true, Client);
            inGame = false;
        }
        else
        {
            Debug.LogWarning("Not in game");
        }

    }

    public static void RequestLobbyData()
    {
        SendData((int)PacketType.LOBBY_DATA, "", true, Client);

    }
    public static void RequestSessionData()
    {
        SendData((int)PacketType.LOBBY_DATA, "", true, Client);

    }

}