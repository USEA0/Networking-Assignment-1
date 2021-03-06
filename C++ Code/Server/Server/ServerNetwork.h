#include <ws2tcpip.h>
#include <string>
#include <iostream>
#include <thread>
#include <queue>
#include "Packet.h"
#include "Tokenizer.h"
#include "GameData.h"

#pragma comment (lib, "ws2_32.lib")
#define DEFAULT_PORT "6883" 

using namespace std;

struct UserProfile {
	int index;
	string Username;
	sockaddr_in udpAddress;
	SOCKET tcpSocket;
	string clientIP;
	int clientLength;

	bool inGame = false;
	int gameNumber = -1;
	//checks for disconnection
	bool active = false;
};

#pragma once
class ServerNetwork
{
public:
	ServerNetwork();
	~ServerNetwork();

	//UDP Socket
	SOCKET udp;
	sockaddr_in serverUDP;
	int clientLength;

	SOCKET tcp;
	sockaddr_in serverTCP;
	//master list of tracked TCP sockets
	fd_set master;

	bool listening = true;

	vector<Packet> packetsIn;

	int clientCount = 0;

	std::vector<UserProfile> ConnectedUsers;
	
	//games currently active by index
	std::vector<std::vector<int>> ActiveGames;




public:
	//accept and save new socket
	void acceptNewClient(std::vector<std::string> data, sockaddr_in address, int length);

	//begin listening to input signals
	void startUpdates();

	//send to all clients
	void sendToAll(Packet pack);
	//send to sepific client(udp) (should not be used)
	void sendTo(Packet pack, int clientID);

	//create packet
	Packet createPacket(PacketType type, string data, int sender);

	//send to all except a client
	void relay(Packet pack, bool useTCP = false);
	//print to cout
	void printOut(Packet pack, int clientID);
	//tcp send to
	void sendTo(Packet pack, SOCKET client);

	void ProcessTCP(Packet pack);
	void ProcessUDP(Packet pack);

	//enter into a game
	void JoinGame(int requester, int responder);

	//TODO: send data about all users
};

