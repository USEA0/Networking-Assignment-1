#include "ClientNetwork.h"


//constructor wrapper
CNET_H ClientNetwork* CreateClient() {
	return new ClientNetwork();
}
//destructor wrapper
CNET_H void DeleteClient(ClientNetwork* client) {
	delete client;
}
//connection
CNET_H void Connect(char* ip, char* username, ClientNetwork* client)
{
	string _ip = string(ip);
	client->username = string(username);

	client->connectToServer(_ip);
}

CNET_H void StartUpdating(ClientNetwork* client)
{
	client->startUpdates();
}

CNET_H void SendData(int type, char* message, bool useTCP, ClientNetwork* client)
{
	client->sendData((PacketType)type, message, useTCP);
}

CNET_H int GetPlayerNumber(ClientNetwork* client)
{
	return client->index;
}

CNET_H void SetupMessage(void(*action)(int type, int sender, char* data))
{
	Message = action;
}

CNET_H void SetupOnConnect(void(*action)()) 
{
	onConnect = action;
}


ClientNetwork::ClientNetwork()
{
	//1: Start Winsock
	WSADATA data;
	WORD version = MAKEWORD(2, 2);

	//startup
	int wsOk = WSAStartup(version, &data);
	if (wsOk != 0) {
		cout << "cant start winsock" << wsOk;
		return;
	}

	//2. setup server information
	serverUDP.sin_family = AF_INET;
	serverUDP.sin_port = htons(54222);
	serverlength = sizeof(serverUDP);

	udp = socket(AF_INET, SOCK_DGRAM, 0);

	tcp = socket(AF_INET, SOCK_STREAM, 0);
	if (tcp == INVALID_SOCKET)
	{
		cerr << "Can't create socket, Err #" << WSAGetLastError() << endl;
		WSACleanup();
		return;
	}
	serverTCP.sin_family = AF_INET;
	serverTCP.sin_port = htons(54223);
}

ClientNetwork::~ClientNetwork()
{
	listening = false;
	closesocket(tcp);
	closesocket(udp);
	WSACleanup();
}

int ClientNetwork::connectToServer()
{
	return connectToServer(addressDefault);
}

int ClientNetwork::connectToServer(string ip)
{
	ipActual = ip;

	inet_pton(AF_INET, ip.c_str(), &serverTCP.sin_addr);		//connecting to the tcp server

	int connResult = connect(tcp, (sockaddr*)&serverTCP, sizeof(serverTCP));
	if (connResult == SOCKET_ERROR)
	{
		cerr << "Can't connect to server, Err #" << WSAGetLastError() << endl;
		closesocket(tcp);
		WSACleanup();
		return SOCKET_ERROR;
	}

	//ping and determine client index
	return 0;
}

int ClientNetwork::sendData(int packetType, string message, bool useTCP)
{
	//create packet
	Packet packet;
	strcpy_s(packet.data, message.c_str() + '\0');
	packet.packet_type = packetType;
	packet.sender = index;

	//set size
	const unsigned int packet_size = sizeof(packet);
	char packet_data[packet_size];

	//seralize
	packet.serialize(packet_data);

	int sendOK = 0;

	//udp send
	if (!useTCP) {
		sendOK = sendto(udp, packet_data, packet_size, 0, (sockaddr*)&serverUDP, sizeof(serverUDP));
		if (sendOK == SOCKET_ERROR) {
			cout << "Send Error: " << WSAGetLastError() << endl;
			return -1;
		}
	}
	//tcp send
	else {
		int sendResult = send(tcp, packet_data, packet_size, 0);
		if (sendResult == SOCKET_ERROR)
		{
			cout << "Send Error: " << WSAGetLastError() << endl;
			return -1;
		}
	}
	return sendOK;
}

void ClientNetwork::startUpdates()
{
	//multithread
	thread udpUpdate = thread([&]() {
		char* buf = new char[MAX_PACKET_SIZE];

		while (listening) {
			//recieve messages
			int length = recvfrom(udp, buf, MAX_PACKET_SIZE, 0, (sockaddr*)&serverUDP, &serverlength);
			if (length != SOCKET_ERROR) {
				Packet packet;
				packet.deserialize(buf);
				ProcessUDP(packet);
			}
		}
		});
	udpUpdate.detach();

	thread tcpUpdate = thread([&]() {
		char* buf = new char[MAX_PACKET_SIZE];

		while (listening) {
			//recieve packets
			int length = recv(tcp, buf, MAX_PACKET_SIZE, 0);
			if (length != SOCKET_ERROR) {
				Packet packet;
				packet.deserialize(buf);
				ProcessTCP(packet);
			}
		}

		});
	tcpUpdate.detach();
}
std::vector<std::string> ClientNetwork::tokenize(char token, std::string text)
{
	std::vector<std::string> temp;
	int lastTokenLocation = 0;

	for (int i = 0; i < text.size(); i++) {
		if (text[i] == token) {
			temp.push_back(text.substr(lastTokenLocation, i - lastTokenLocation));
			lastTokenLocation = i + 1;

		}
	}
	temp.push_back(text.substr(lastTokenLocation, text.size()));

	return temp;
}

void ClientNetwork::ProcessTCP(Packet pack)
{
	std::vector<std::string> parsedData;
	parsedData = tokenize(',', pack.data);

	switch (pack.packet_type) {
	case PacketType::INIT_CONNECTION:
		//filter by sender
		if (pack.sender == -1) {
			index = std::stof(parsedData[0]);

			//connect to udp
			inet_pton(AF_INET, ipActual.c_str(), &serverUDP.sin_addr);
			sendData(INIT_CONNECTION, to_string(index) + "," + username, false);
		}
		break;
	case PacketType::MESSAGE:
	case PacketType::REQUEST_GAME:
	case PacketType::REQUEST_RESPONSE:
	case PacketType::SESSION_DATA:
	case PacketType::LOBBY_DATA:

		Message(pack.packet_type, pack.sender, pack.data);
		break;

	default:
		cout << "Error Unhandled Type";

		break;
	}
}

void ClientNetwork::ProcessUDP(Packet pack)
{
	std::vector<std::string> parsedData;
	parsedData = tokenize(',', pack.data);

	switch (pack.packet_type) {
	case PacketType::INIT_CONNECTION:
		index = std::stof(parsedData[0]);
		//call action
		onConnect();
		break;
	case PacketType::MESSAGE:
		//send UDP message
		Message((int)PacketType::MESSAGE, pack.sender, pack.data);
		break;
	default:
		cout << "Error Unhandled Type";

		break;
	}

}
