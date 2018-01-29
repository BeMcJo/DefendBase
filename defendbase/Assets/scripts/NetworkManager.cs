﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player
{
    public int connectionID;
    public string playerName;
    public GameObject playerGO;
}

public class Host
{
    public string ip;
    public int port;

    public Host()
    {
        ip = "";
        port = -1;
    }

    public Host(string addr, int p)
    {
        ip = addr;
        port = p;
    }

    public bool Equals(Host h)
    {
        return h.ip.Equals(ip) && port == h.port;
    }
}

public class NetworkManager : MonoBehaviour {
    public static NetworkManager nm;

    // Max number of host connections
    private const int MAX_CONNECTION = 100;
    private const int MAX_PLAYERS = 6;

    private int SERVER_PORT = 5920,
                CLIENT_PORT = 5919;
    public int broadcastPort = 47777;
    public int broadcastKey = 1000;
    public int broadcastVersion = 1;
    public int broadcastSubVersion = 1;

    private int hostID;
    private int recHostID;
    private int webHostID;
    //private int connectionID;
    private string serverIP;
    private int serverPort;
    private Host connectedHost;

    private int ourClientID = 0;
    // Used for connecting for important data (Paypal, payments...)
    private int reliableChannel;
    // Used for less important data that can tolerate missing packets/corruption
    private int unreliableChannel;


    private float connectionTime;
    private float timer, timeout = 30f;
    private bool isStarted = false;
    public bool inLobby = false;
    private bool requestingConnection = false;
    private bool inGame = false;
    private bool isConnected = false;
    private bool destroyed = false;
    private bool isOriginal = false;
    private bool isDisconnected = false;
    private bool initialConfirmation = true;

    private bool isBroadcasting = false;
    private bool isHost = false;
    private bool isListening = false;
    private bool gameStarted = false;
    private bool doneSetup = false;
    private bool findingHosts = false;


    private string lastRequest = "NOREQUESTS|";
    //private byte error;

    // cnnID -> Player
    private Dictionary<int, Player> players = new Dictionary<int, Player>();
    // cnnID -> Player Order Pos
    private Dictionary<int, int> client2player = new Dictionary<int, int>();
    // Player Order Pos -> cnnID
    private Dictionary<int, int> player2client = new Dictionary<int, int>();
    // Player Order Pos -> DC timer
    private Dictionary<int, float> playerDC = new Dictionary<int, float>();
    //private List<Player> playerOrder = new List<Player>();

    public Dictionary<Host, GameObject> hosts = new Dictionary<Host, GameObject>();

    private float lastMovementUpdate;
    //private float timer, timeout = 30f;
    private float movementUpdateRate = 0.05f;

    private byte[] msgOutBuffer = new byte[1024];
    private string awaitingResponseFor;


    public Transform hostsList;
    public GameObject playerPrefab, joinButtonPrefab;

    public List<string> activityLog;

    //public GameObject playerPrefab;
    GameObject //startBroadcastButton,
               //stopBroadcastButton,
               leaveLobbyBtn,
               lobbySearchCanvas,
               startGameBtn,
               settingsBtn,
               playersList,
               playerSpawnPoints,
               lobbyCanvas;
    //multiplayersCanvas;
    private Player myPlayer;


    int recHostId;
    int connectionId;
    int channelId;
    byte[] recBuffer = new byte[1024];
    int bufferSize = 1024;
    int dataSize;
    byte error;

    private void Start()
    {
        if (!nm)
        {
            nm = this;
            DontDestroyOnLoad(nm);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        //LoadLobbyScene();
        //LoadSceneObjects();
        NetworkTransport.Init();
        //multiplayersCanvas = GameObject.Find("MultiplayersCanvas") as GameObject;
        //multiplayersCanvas.SetActive(false);

        /*
        ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

        hostId = NetworkTransport.AddHost(topo, 0);

        
        StartBroadcast();
        
        */


        //webHostId = NetworkTransport.AddWebsocketHost(topo, port);

        /*ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

        hostId = NetworkTransport.AddHost(topo, port);
        webHostId = NetworkTransport.AddWebsocketHost(topo, port);

        byte error;
        msgOutBuffer = Encoding.Unicode.GetBytes("SERVER|");
        Debug.Log("braodcast");
        if (!NetworkTransport.StartBroadcastDiscovery(hostId, broadcastPort, broadcastKey, broadcastVersion, broadcastSubVersion, msgOutBuffer, msgOutBuffer.Length, 1000, out error))
        {
            Debug.LogError("NetworkDiscovery StartBroadcast failed err: " + error);
            return;
        }*/

        //isStarted = true;
    }

    private void Update()
    {
        //Debug.Log("Time" + Time.time);
        //Debug.Log("DeltaTime" + Time.deltaTime);
        if (!isStarted)// || !isListening)
        {
            return;
        }
        if (isHost)
        {
            PerformHostActivities();
        }
        else
        {
            PerformClientActivities();
        }
        //if (!gameStarted)
        //{
        //    return;
        //}

        if (GameManager.gm.inGame)
        {
            /*string s = "";
            foreach (string str in activityLog)
                s += str + "\n";
            GameManager.gm.gameCanvas.transform.Find("log").GetChild(0).GetComponent<Text>().text = s;
            */
            //List<int> dc = new List<int>();
            /*foreach (KeyValuePair<int, float> kvp in playerDC)
            {
                Debug.Log(Time.time - kvp.Value + " " + kvp.Key);
                if (Time.time - kvp.Value >= timeout)
                    LeaveGame();
            }*/
        }

        
        /*
        // Ask player for their position
        if (Time.time - lastMovementUpdate > movementUpdateRate && clients.Count > 0)
        {
            lastMovementUpdate = Time.time;
            string m = "ASKPOSITION|";
            foreach (Player sc in clients)
            {
                m += sc.connectionId.ToString() + '%' + sc.position.x.ToString() + '%' + sc.position.y.ToString() + '|';
            }
            m = m.Trim('|');
            Send(m, unreliableChannel, clients);
        }*/

    }

    public void StartUpNetworkActivities()
    {
        isStarted = true;
        GameManager.gm.lobbyCanvas.transform.Find("BackBtn").GetComponent<Button>().interactable = true;
        GameManager.gm.lobbyCanvas.transform.Find("ReadyBtn").GetComponent<Button>().interactable = true;
    }

    public void SetupAsHost()
    {
        isHost = true;
        ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);

        hostID = NetworkTransport.AddHost(topo, 0);
        ourClientID = hostID;
        SpawnPlayerUI("HOST", ourClientID);

        StartBroadcast();
    }

    public void StartBroadcast()
    {
        byte error;
        msgOutBuffer = Encoding.Unicode.GetBytes("HOST|");// + GameManager.gm.playerName + "|");
        if (!NetworkTransport.StartBroadcastDiscovery(hostID, broadcastPort, broadcastKey, broadcastVersion, broadcastSubVersion, msgOutBuffer, msgOutBuffer.Length, 1000, out error))
        {
            Debug.LogError("NetworkDiscovery StartBroadcast failed err: " + error);
            return;
        }
        isBroadcasting = true;
        //isListening = true;
    }

    public void StopBroadcast()
    {
        NetworkTransport.StopBroadcastDiscovery();
        isBroadcasting = false;
    }

    public void SetupAsClient()
    {
        isHost = false;
        requestingConnection = false;
        inLobby = false;
        hostID = -1;
    }

    public void StopFindingHosts()
    {
        if (findingHosts)
        {
            findingHosts = false;
            if (hostID != -1)
            {
                NetworkTransport.RemoveHost(hostID);
            }
            hostID = -1;

            //lobbySearchCanvas.transform.Find("FindHostBtn").gameObject.SetActive(true);
            //lobbySearchCanvas.transform.Find("StopSearchBtn").gameObject.SetActive(false);
        }
        isStarted = false;
    }

    public void FindHosts()
    {
        //Debug.Log("Finding Hosts");
        if (!findingHosts)
        {
            findingHosts = true;
            //lobbySearchCanvas.transform.Find("FindHostBtn").gameObject.SetActive(false);
            //lobbySearchCanvas.transform.Find("StopSearchBtn").gameObject.SetActive(true);

        }
        // Does player have name?
        /*string pName = GameObject.Find("NameInput").GetComponent<InputField>().text;
        pName = "LOLS";
        if (pName == "")
        {
            Debug.Log("You must enter a name");
            return;
        }*/
        //playerName = pName;
        // Does player know who the host is?
        if (hostID == -1)
        {
            //Debug.Log("Haven't received port");
            ConnectionConfig cc = new ConnectionConfig();

            reliableChannel = cc.AddChannel(QosType.Reliable);
            unreliableChannel = cc.AddChannel(QosType.Unreliable);

            HostTopology topo = new HostTopology(cc, MAX_CONNECTION);
            hostID = NetworkTransport.AddHost(topo, broadcastPort);
            //Debug.Log(hostId);
            if (hostID == -1)
            {
                //NetworkTransport.RemoveHost(hostID);
                Debug.LogError("NetworkDiscovery StartAsClient - addHost failed");
                return;
            }
        }
        NetworkTransport.SetBroadcastCredentials(hostID, broadcastKey, broadcastVersion, broadcastSubVersion, out error);

        //int counter = 0;
        NetworkEventType recData = NetworkEventType.Nothing;
        //do
        // {
        //Debug.Log("Listening...");
        recData = NetworkTransport.Receive(out recHostID, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        //Debug.Log(recData);
        if (recData == NetworkEventType.BroadcastEvent)
        {
            //Debug.Log("YE");

            int rcvSize;
            NetworkTransport.GetBroadcastConnectionMessage(hostID, recBuffer, recBuffer.Length, out rcvSize, out error);
            string senderAddr;
            int senderPort;
            NetworkTransport.GetBroadcastConnectionInfo(hostID, out senderAddr, out senderPort, out error);
            //Debug.Log(rcvSize);
            OnReceivedBroadcast(senderAddr, senderPort, Encoding.Unicode.GetString(recBuffer));
        }
        /*counter++;
        //Waits roughly 10seconds before timeout
        if (counter > 1 << 24 - 1)
        {
            Debug.Log("WAITED TOO LONG");
            NetworkTransport.RemoveHost(hostId);
            hostId = -1;
            return;
        }*/
        // } while (recData != NetworkEventType.BroadcastEvent);
    }

    public void ConnectTo(Host host)
    {
        //Debug.Log("Connecting" + host == null);
        connectedHost = host;
        //Debug.Log(host != null);
        if (hostID != -1)
            NetworkTransport.RemoveHost(hostID);

        ConnectionConfig cc = new ConnectionConfig();

        reliableChannel = cc.AddChannel(QosType.Reliable);
        unreliableChannel = cc.AddChannel(QosType.Unreliable);

        HostTopology topo = new HostTopology(cc, MAX_CONNECTION);
        hostID = NetworkTransport.AddHost(topo, 0);
        if (hostID == -1)
        {
            Debug.LogError("NetworkDiscovery StartAsClient - addHost failed");
            return;
        }
        connectionId = NetworkTransport.Connect(hostID, connectedHost.ip, connectedHost.port, 0, out error);
        //isConnected = true;
        timer = Time.time;
        requestingConnection = true;
    }

    void OnPurpose()
    {
        if (!GameManager.gm.inGame)
        {
            Send("NEWCNN|", reliableChannel);
        }
        else
        {
            Debug.Log("Reconnected");
            //Send("RECONNECT|" + GameManager.gm.myTurn + "|" + activityLog.Count, reliableChannel);
            isDisconnected = false;
            requestingConnection = false;
        }
    }

    public void OnReceivedBroadcast(string fromAddress, int fromPort, string data)
    {

        string[] splitAddr = fromAddress.Split(':');
        string[] splitData = data.Split('|');

        switch (splitData[0])
        {
            case "HOST":
                //Debug.Log("Found Connection");
                string tempIp = splitAddr[splitAddr.Length - 1];
                //hostPort;
                //Debug.Log(serverIp);
                int tempPort = fromPort;
                //Debug.Log(serverPort);
                Host host = new Host(tempIp, tempPort);
                bool newHost = true;
                foreach (KeyValuePair<Host, GameObject> kvp in hosts)
                {
                    if (kvp.Key.Equals(host))
                    {
                        //Debug.Log("EXITST");

                        hosts[kvp.Key].transform.GetComponent<ExpiringButton>().Reset();
                        newHost = false;
                        break;
                    }
                }
                if (newHost)
                {
                    //Debug.Log("New host");
                    GameObject joinBtn = Instantiate(GameManager.gm.buttonPrefab) as GameObject;
                    joinBtn.AddComponent<ExpiringButton>();
                    joinBtn.transform.SetParent(GameManager.gm.hostListCanvas.transform.Find("ButtonsContainer"));
                    joinBtn.transform.GetComponent<ExpiringButton>().SetExpiringButton(host);
                    hosts.Add(host, joinBtn);
                    joinBtn.transform.Find("Text").GetComponent<Text>().text = "Game";
                    //joinBtn.transform.Find("Text").GetComponent<Text>().text = "Room #" + host.port + ", " + host.ip;
                    joinBtn.transform.localScale = new Vector3(1, 1, 1);
                }

                break;
        }
    }

    public void ClearHostList()
    {
        //List<Host> expiredHosts = new List<Host>();
        foreach (KeyValuePair<Host, GameObject> kvp in hosts)
        {
            Destroy(kvp.Value);
        }
        hosts.Clear();
    }

    private void OnConnection(int cnnId)
    {
        //Debug.Log("CONNECTION ESTABLISHED" + gameStarted);
        // Cannot play with more than 5 players total
        if (players.Count == MAX_PLAYERS)
        {
            Send("FULLROOM|", reliableChannel, cnnId);
            return;
        }
        // Cannot play with more than 5 players total
        if (gameStarted)
        {
            Send("FULLROOM|", reliableChannel, (int)cnnId);
            return;
        }
        // Add him to list
        Player c = new Player();
        c.connectionID = cnnId;
        c.playerName = "TEMP";
        c.playerGO = Instantiate(GameManager.gm.playerUIPrefab);
        c.playerGO.SetActive(false);
        players.Add(cnnId, c);
        if (players.Count == MAX_PLAYERS - 1)
        {
            StopBroadcast();
            //Send("FULLROOM|", reliableChannel, cnnId);
        }
        // When player joins server, tell him his ID
        // Request his name and send name of all other players
        string msg = "ASKNAME|" + cnnId + "|";// + myPlayer.playerName + "%0|";
        foreach (KeyValuePair<int, Player> kvp in players)
        {
            if (kvp.Key != cnnId)
            {
                Player sc = kvp.Value;
                msg += sc.playerName + "%" + sc.connectionID + "|";
            }
        }
        msg = msg.Trim('|');

        // ASKNAME|3|DAVE%1|Micheal%2|TEMP%3
        Send(msg, reliableChannel, cnnId);
    }

    private void OnNameIs(int cnnId, string playerName)
    {
        if (players.Count == 1 && !players.ContainsKey(cnnId))
            return;
        Player sc = players[cnnId];
        //Player sc = players.Find(x => x.connectionId == cnnId);
        sc.playerName = playerName;
        sc.playerGO.transform.Find("NameText").GetComponent<Text>().text = playerName;
        sc.playerGO.transform.SetParent(GameManager.gm.lobbyCanvas.transform.Find("PlayerList"));//GameManager.gm.playerRotation.transform);
        //sc.playerGO.transform.localScale = new Vector3(1, 1, 1);
        //lobbyCanvas.transform.Find("StartBtn").GetComponent<Button>().interactable = true;
        sc.playerGO.SetActive(true);

        // Tell everybody that new player has connected
        Send("CNN|" + playerName + '|' + cnnId, reliableChannel, players);
    }

    private void OnDisconnection(int cnnId)
    {
        if (players.Count == 1 || !players.ContainsKey(cnnId))
            return;
        Destroy(players[cnnId].playerGO);
        // Remove this player from our client list
        players.Remove(cnnId);

        //lobbyCanvas.transform.Find("StartBtn").GetComponent<Button>().interactable = players.Count > 1;
        if (!gameStarted && !isBroadcasting)
        {
            StartBroadcast();
        }
        Debug.Log("DCS: " + cnnId);
        // Tell everyone that someone has disconnected
        Send("DC|" + cnnId, reliableChannel, players);
    }

    public void PerformHostActivities()
    {
        /*foreach(KeyValuePair<int,Player> kvp in players)
        {
            Debug.Log("HOSTING:" + kvp.Key + " " + kvp.Value.connectionID);
        }*/
       // Debug.Log("s");
        //if (!isListening)
        //{
        //    return;
        //}
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        //Debug.Log(recHostId + " " + connectionId + " " + channelId + " " + recBuffer + " " + bufferSize + " " + dataSize + " " + error);

        if (recData != NetworkEventType.Nothing)
            Debug.Log(recData);
        switch (recData)
        {
            case NetworkEventType.ConnectEvent:    //2
                Debug.Log("Player " + connectionId + " has connected");
                Send("PURPOSE|", reliableChannel, connectionId);
                //OnConnection(connectionId);
                break;
            case NetworkEventType.DataEvent:       //3
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                Debug.Log("Player " + connectionId + " has sent: " + msg);
                string[] splitData = msg.Split('|');

                switch (splitData[0])
                {
                    /*case "DONESETUP":
                        OnDoneSetup(connectionId);
                        break;*/
                    case "LEAVELOBBY?":
                        Debug.Log("Disconnectin gplayer" + connectionId);
                        Send("DC|" + connectionId, reliableChannel, connectionId);
                        OnDisconnection(connectionId);
                        Debug.Log("Done");
                        break;

                    case "NAMEIS":
                        OnNameIs(connectionId, splitData[1]);
                        break;
                        /*
                    case "NEEDDECK":
                        OnNeedDeck(connectionId);
                        break;
                    case "NEEDDECKCOUNT":
                        Send("DECKCOUNT|" + GameManager.gm.deck.Count(), reliableChannel, connectionId);
                        break;
                    case "NEEDHAND":
                        OnNeedHand(connectionId, splitData);
                        break;
                    case "NEEDPLAYERORDER":
                        OnNeedPlayerOrder(connectionId);
                        break;
                    case "NEEDTRUMP":
                        Send("TRUMP|" + GameManager.gm.trump.ToString(), reliableChannel, connectionId);
                        break;
                        */
                    case "NEWCNN":
                        OnConnection(connectionId);
                        break;
                        /*
                    case "PLAYERATTACK":
                        if (OnReceivePlayerAttack(connectionId, splitData))
                            Send(msg, reliableChannel, players);
                        else
                            Send("PLAYERATTACKFAILED|", reliableChannel, connectionId);
                        break;
                    case "PLAYERENDBATTLE":
                        //        Debug.Log("RECEVED ENBGBATTLE");
                        GameManager.gm.PerformCommitEndBattle();
                        Send(msg, reliableChannel, players);
                        activityLog.Add(msg);
                        break;
                    case "PLAYERDEFEND":
                        if (OnReceivePlayerDefend(connectionId, splitData))
                        {
                            Send(msg, reliableChannel, players);
                            activityLog.Add(msg);
                        }
                        else
                            Send("PLAYERDEFENDFAILED|", reliableChannel, connectionId);
                        break;
                    case "PLAYERTRANSFER":
                        if (OnPlayerTransfer(connectionId, splitData))
                        {
                            Send(msg, reliableChannel, players);
                            activityLog.Add(msg);
                        }
                        else
                            Send("PLAYERTRANSFERFAILED|", reliableChannel, connectionId);
                        break;
                    case "RECONNECT":
                        OnReconnect(connectionId, splitData);
                        break;
                    case "STARTGAME":
                        OnReceivedStartGame(connectionId);
                        break;
                    default:
                        Debug.Log("Invalid Message: " + msg);
                        break;
                        */
                }
                break;
            case NetworkEventType.DisconnectEvent: //4
                Debug.Log("Player " + connectionId + " has disconnected");
                if (gameStarted)
                {
                    //if (GameManager.gm.durak != -1)
                    //    return;
                    playerDC.Add(client2player[connectionId], Time.time);
                    //LeaveGame();
                }
                else
                {
                    OnDisconnection(connectionId);
                }
                break;
        }
    }

    public void PerformClientActivities()
    {
        /*foreach (KeyValuePair<int, Player> kvp in players)
        {
            Debug.Log("CLIENTING:" + kvp.Key + " " + kvp.Value.connectionID);
        }*/
        if (hosts.Count > 0)
        {
            List<Host> expiredHosts = new List<Host>();
            foreach (KeyValuePair<Host, GameObject> kvp in hosts)
            {
                kvp.Value.transform.GetComponent<ExpiringButton>().Decrement();
                //Debug.Log(kvp.Value.transform.GetComponent<ExpiringButton>().timer);
                if (kvp.Value.transform.GetComponent<ExpiringButton>().Expired())
                {
                    Destroy(kvp.Value);
                    expiredHosts.Add(kvp.Key);
                }
            }
            for (int i = 0; i < expiredHosts.Count; i++)
            {
                hosts.Remove(expiredHosts[i]);
            }
        }
        //Debug.Log(requestingConnection + " " + findingHosts + " " + inGame);
        if (!requestingConnection && !inLobby && !inGame)// || findingHosts)
        {
            //Debug.Log("Searching");
            FindHosts();
            return;
        }
        //Debug.Log("waiting for requests");
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
        //Debug.Log(recHostId + " " + connectionId + " " + channelId + " " + recBuffer + " " + bufferSize + " " + dataSize + " " + error);

        switch (recData)
        {
            case NetworkEventType.ConnectEvent:
                Debug.Log("I CONNE");
                break;
            case NetworkEventType.DataEvent:       //3
                //Debug.Log("data");
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                Debug.Log("Receiving : " + msg);
                string[] splitData = msg.Split('|');

                switch (splitData[0])
                {
                    /*case "ANYREQUESTS":
                        OnAnyRequests();
                        break;*/
                    case "ASKNAME":
                        OnAskName(splitData);
                        break;
                        /*
                    case "ASKPOSITION":
                        OnAskPosition(splitData);
                        break;*/
                    case "CNN":
                        SpawnPlayerUI(splitData[1], int.Parse(splitData[2]));
                        break;
                        
                    case "DC":
                        PlayerDisconnected(int.Parse(splitData[1]));
                        break;
                        /*
                    case "DECK":
                        ReceiveDeck(splitData);
                        break;
                    case "DECKCOUNT":
                        ReceiveDeckCount(splitData[1]);
                        break;
                    case "DONESETUP":
                        ConfirmDoneSetup();
                        break;
                    case "FULLROOM":
                        Disconnect();
                        //lobbySearchCanvas.transform.Find("StopSearchBtn").gameObject.SetActive(true);
                        FindHosts();
                        break;
                    case "HAND":
                        ReceiveHand(splitData);
                        break;*/
                    case "LEAVELOBBY":
                        LeaveLobby();
                        break;

                    /*
                case "LEAVEGAME":
                    LeaveGame();
                    break;
                case "PLAYERATTACK":
                    OnReceivePlayerAttack(splitData);
                    activityLog.Add(msg);
                    break;
                case "PLAYERATTACKFAILED":
                    Debug.Log("failed to attak");
                    break;
                case "PLAYERENDBATTLE":
                    // Debug.Log("RECEVED ENBGBATTLE");
                    GameManager.gm.PerformCommitEndBattle();
                    activityLog.Add(msg);
                    if (int.Parse(splitData[1]) == GameManager.gm.myTurn)
                        lastRequest = "NOREQUEST|" + GameManager.gm.myTurn;
                    break;
                case "PLAYERDEFEND":
                    OnReceivePlayerDefend(splitData);
                    activityLog.Add(msg);
                    break;
                case "PLAYERHAND":
                    ReceivePlayersHands(splitData);
                    break;
                case "PLAYERORDER":
                    // Debug.Log("P ORDER");
                    //foreach (string s in splitData)
                    //    Debug.Log(s);
                    ReceivePlayerOrder(splitData);
                    break;
                case "PLAYERSTARTTURN":
                    OnPlayerStartTurn(int.Parse(splitData[1]));
                    activityLog.Add(msg);
                    break;
                case "PLAYERTRANSFER":
                    OnPlayerTransfer(splitData);
                    activityLog.Add(msg);
                    break;
                    */
                    case "PURPOSE":
                        OnPurpose();
                        break;
                        /*
                    case "STARTGAME":
                        StartGame();
                        break;
                    case "TRUMP":
                        ReceiveTrumpCard(splitData[1]);
                        break;
                    default:
                        Debug.Log("Invalid Message: " + msg);
                        break;
                        */
                }
                break;
            case NetworkEventType.BroadcastEvent:
                /*Debug.Log("DISCOVER");
                //string m = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                int rcvSize;
                NetworkTransport.GetBroadcastConnectionMessage(hostId, buffer, buffer.Length, out rcvSize, out error);
                string senderAddr;
                int senderPort;
                NetworkTransport.GetBroadcastConnectionInfo(hostId, out senderAddr, out senderPort, out error);
               
                OnReceivedBroadcast(senderAddr, senderPort, Encoding.Unicode.GetString(buffer));
                //connectionId = NetworkTransport.Connect(hostId, "", port, 0, out error);
                //Debug.Log(m);*/
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("SOMEONE DC");
                if (connectionId == ourClientID)
                {
                    Debug.Log("HOST DC?");
                    LeaveLobby();
                    /*if (GameManager.gm.durak != -1)
                        return;
                    Debug.Log("THATS OUR MAIN CON");
                    isDisconnected = true;
                    timer = Time.time;
                    ReconnectTo(connectedHost);
                    //LeaveGame();*/
                }else if (requestingConnection)
                {
                    //Debug.Log(Time.time - timer + " " + timeout);
                    //if (Time.time - timer >= timeout)
                    //{
                        Debug.Log("WAITED TOO LONG");
                    Disconnect();
                    SetupAsClient();
                    StartUpNetworkActivities();    
                    //isConnected = false;
                        //findingHosts = true;
                        //requestingConnection = false;
                        //inLobby = false;
                        //inGame = false;
                    //}
                }
                break;
        }


        if (requestingConnection)
        {
            
        }
    }

    private void PlayerDisconnected(int cnnId)
    {
        
        if(cnnId == ourClientID)
        {
            Debug.Log("Leaving from player dc");
            LeaveLobby();
            return;
        }
        Debug.Log("NOt us");
        Destroy(players[cnnId].playerGO);
        players.Remove(cnnId);
    }

    private void OnAskName(string[] data)
    {
        //GameManager.gm.GoToLobbyScene();

        //if (cnnId == ourClientID)
        //{
            // Add mobility
            //go.AddComponent<PlayerMotor>();
            GameManager.gm.ToggleHostListCanvas();
            GameManager.gm.ToggleLobbyCanvas();
            //lobbyCanvas.SetActive(true);
            //lobbySearchCanvas.SetActive(false);
            //inGameCanvas.SetActive(true);
            inLobby = true;
            requestingConnection = false;
        //    playerName = "Player"; //GameManager.gm.playerName;
            //isStarted = true;
       // }

        // Set this client's ID
        ourClientID = int.Parse(data[1]);

        // Send our name to server
        Send("NAMEIS|" + "PLAYER", reliableChannel);

        // Create all the other players
        for (int i = 2; i < data.Length; i++)
        {
            string[] d = data[i].Split('%');
            SpawnPlayerUI(d[0], int.Parse(d[1]));
        }

    }

    private void SpawnPlayerUI(string playerName, int cnnId)
    {
        
        Debug.Log("Spawn player " + playerName + cnnId);
        GameObject go = Instantiate(GameManager.gm.playerUIPrefab);
        // Our client?
        if (cnnId == ourClientID)
        {
            // Add mobility
            //go.AddComponent<PlayerMotor>();
            //GameManager.gm.ToggleHostListCanvas();
            //GameManager.gm.ToggleLobbyCanvas();
            //lobbyCanvas.SetActive(true);
            //lobbySearchCanvas.SetActive(false);
            //inGameCanvas.SetActive(true);
            //inLobby = true;
            //requestingConnection = false;
            playerName = "ME"; //GameManager.gm.playerName;
            //isStarted = true;
        }
        Player p = new Player();
        p.playerGO = go;
        p.playerName = playerName;
        p.connectionID = cnnId;
        //p.playerGO.GetComponentInChildren<TextMesh>().text = playerName;
        p.playerGO.transform.SetParent(GameManager.gm.lobbyCanvas.transform.Find("PlayerList"));//GameManager.gm.playerRotation.transform);
        p.playerGO.transform.Find("NameText").GetComponent<Text>().text = playerName;
        go.transform.localScale = new Vector3(1, 1, 1);
        players.Add(cnnId, p);
    }

    public void Disconnect()
    {
        if (hostID != -1)
        {
            if(!isConnected)
                NetworkTransport.RemoveHost(hostID);
            else
                NetworkTransport.Disconnect(hostID, connectionId, out error);
        }
        hostID = -1;
        findingHosts = false;
        connectedHost = null;
        isStarted = false;
        Debug.Log("Disconnected");
        isConnected = false;
    }

    public void RequestAction(string action)
    {
        Send(action + "?|", reliableChannel);
    }

    public void RequestLeaveLobby()
    {
        if (isHost)
        {
            LeaveLobby();
            return;
        }
        RequestAction("LEAVELOBBY");
        GameManager.gm.lobbyCanvas.transform.Find("BackBtn").GetComponent<Button>().interactable = false;
        GameManager.gm.lobbyCanvas.transform.Find("ReadyBtn").GetComponent<Button>().interactable = false;
    }

    private void OnLevelWasLoaded(int level)
    {
        if (this != nm)
            return;
        //Debug.Log("Here" + level);
        switch (level)
        {
            // Main
            case 0:
                LoadMainScene();
                //Destroy(gameObject);
                //destroyed = true;
                break;
            // Game
            case 1:
                //Destroy(gameObject);
                //destroyed = true;
                break;
            // Calibration
            case 2:
                break;
            // Lobby
            case 3:
                LoadLobbyScene();
                break;
            // Game (Multiplayer)
            case 4:
                //LoadObjects();
                break;
        }
    }

    public void LeaveLobby()
    {
        Debug.Log("Leaving");
        isStarted = false;
        foreach(KeyValuePair<int,Player> kvp in players)
        {
            Destroy(kvp.Value.playerGO);
        }
        players.Clear();
        if (isBroadcasting)
        {
            StopBroadcast();
        }
        GameManager.gm.ToggleLobbyCanvas();
        GameManager.gm.ToggleMultiplayerCanvas();
        inLobby = false;
        Disconnect();
    }

    public void LoadMainScene()
    {

    }


    public void LoadLobbyScene()
    {
        GameManager.gm.playerRotation = GameObject.Find("Player Rotation");
        playerSpawnPoints = GameManager.gm.playerRotation.transform.Find("Player Spawn Points").gameObject;
        Player p = new Player();
        p.connectionID = ourClientID;
        p.playerName = "You";
        GameObject myPlayer = Instantiate(GameManager.gm.playerPrefab);
        p.playerGO = myPlayer;
        //myPlayer.transform.position = playerSpawnPoints.transform.GetChild(0).position;
        myPlayer.transform.SetParent(GameManager.gm.playerRotation.transform);
        players.Add(ourClientID, p);
        for(int i = 0; i < players.Count; i++)
        {
            players[i].playerGO.transform.position = playerSpawnPoints.transform.GetChild(i).position;
            if(i != ourClientID)
            {
                players[i].playerGO.transform.GetComponent<PlayerController>().playerCam.gameObject.SetActive(false);
            }
        }
        GameManager.gm.playerRotation.transform.eulerAngles = GameManager.gm.playerOrientation;
    }

    private void Send(string message, int channelId)
    {
        Debug.Log("Sending : " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);
        NetworkTransport.Send(hostID, connectionId, channelId, msg, message.Length * sizeof(char), out error);
    }

    private void Send(string message, int channelId, int cnnId)
    {
        //if (!players.ContainsKey(cnnId))
        //    return;
        Debug.Log("send" + message + " " + cnnId);
        NetworkTransport.Send(hostID, cnnId, channelId, Encoding.Unicode.GetBytes(message), message.Length * sizeof(char), out error);
    }

    private void Send(string message, int channelId, Dictionary<int, Player> c)
    {

        Debug.Log("Sending : " + message);
        byte[] msg = Encoding.Unicode.GetBytes(message);
        foreach (KeyValuePair<int, Player> kvp in players)
        {
            Player p = kvp.Value;
            if (p != null && p != myPlayer)
                NetworkTransport.Send(hostID, p.connectionID, channelId, msg, message.Length * sizeof(char), out error);
            //return;
        }
    }
}