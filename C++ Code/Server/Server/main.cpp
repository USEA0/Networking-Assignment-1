#include <iostream>
#include <map>
#include <vector> 
#include "ServerNetwork.h"

using namespace std;

bool running = true;

int main() {

	ServerNetwork net = ServerNetwork();
	net.startUpdates();

}

