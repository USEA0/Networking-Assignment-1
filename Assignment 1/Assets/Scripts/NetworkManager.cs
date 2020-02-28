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

    //if the player is in a game(chat)
    public static bool inGame = false;

    //check if request is active
    public static bool requestActive = false;

    //holds the last active requester index
    public int requesterIndex = -1;

    #endregion

    // Start is called before the first frame update
    void Awake()
    {
        Client = CreateClient();
        SetupMessage(PacketRecieved);
        SetupOnConnect(OnConnect);
    }

    //call this to connect to server
    public void ConnectToServer(string ip, string username)
    {
        Connect(ip, username, Client);
        StartUpdating(Client);
    }

    //this will be called if the connection is successful
    static void OnConnect()
    {
        connected = true;
        playerNumber = GetPlayerNumber(Client);
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
                break;

             //this recieves a game request from another user
            case PacketType.REQUEST_GAME:
                break;

            //this recieves a response to a sent game request
            case PacketType.REQUEST_RESPONSE:
                break;

            //this recieves all the server data from the lobby
            case PacketType.LOBBY_DATA:
                break;

            //this recieves all the session data from the lobby
            case PacketType.SESSION_DATA:
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

}